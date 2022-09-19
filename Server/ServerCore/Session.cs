using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public abstract class Session
    {
        private Socket _socket;
        private int _disconnected = 0;
        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);

        public abstract void OnDisconnected(EndPoint endPoint);
        public abstract void OnReceive(ArraySegment<byte> buffer);

        public abstract void OnSend(int numOfBytes);

        public void Start(Socket socket)
        {
            _socket = socket;
            _receiveArgs = new SocketAsyncEventArgs();
            _receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);            
            _receiveArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            RegisterReceive();
        }

        public void Send(byte[] sendBuffer)
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
                byte[] buffer = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buffer, 0, buffer.Length)); // C# 에선 일반적으로 포인터를 사용할 수 없기때문에 버퍼의일부 범위를 넣기위해서 범위를 지정한 배열의 일부를 생성해서 넣어줌
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
