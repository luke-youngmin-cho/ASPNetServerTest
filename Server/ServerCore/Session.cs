using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{

    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        public sealed override int OnReceive(ArraySegment<byte> buffer)
        {
            int processLength = 0;

            while (true)
            {
                // 최소 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 정상 패킷일경우 
                OnReceivePacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLength += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLength;
        }

        public abstract void OnReceivePacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        private Socket _socket;
        private int _disconnected = 0;

        ReceiveBuffer _receiveBuffer = new ReceiveBuffer(1024);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        bool _pending = false;
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);

        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract int OnReceive(ArraySegment<byte> buffer);

        public abstract void OnSend(int numOfBytes);

        public void Start(Socket socket)
        {
            _socket = socket;
            _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);            
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            RegisterReceive();
        }

        public void Send(ArraySegment<byte> sendBuffer)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);
                if (_pending == false)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            // 멀티쓰레드환경이기떄문에 Disconnect가 여러번 호출될 수 있고 
            // 이미 해제된 객체를 또 해제하려고하면 에러가 뜨기때문에 Interlock 처리해준다.
            if ((Interlocked.Exchange(ref _disconnected, 1) == 1))
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        void RegisterSend()
        {
            _pendingList.Clear();
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buffer = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buffer.Array, buffer.Offset, buffer.Count)); // C# 에선 일반적으로 포인터를 사용할 수 없기때문에 버퍼의일부 범위를 넣기위해서 범위를 지정한 배열의 일부를 생성해서 넣어줌
            }

            _sendArgs.BufferList = _pendingList;

            bool tempPending = _socket.SendAsync(_sendArgs);
            if (tempPending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 &&
                args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        else
                        {
                            _pending = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {

                }
            }
        }

        void RegisterReceive()
        {
            _receiveBuffer.Clear();
            ArraySegment<byte> segment = _receiveBuffer.WriteSegment;
            _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_receiveArgs);
            if (pending == false)
                OnReceiveCompleted(null, _receiveArgs);
        }

        void OnReceiveCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 &&
                    args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동
                    if (_receiveBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLength = OnReceive(_receiveBuffer.ReadSegment);
                    if (processLength < 0 || _receiveBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if (_receiveBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

                    OnReceive(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    RegisterReceive();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnReceiveCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }
}
