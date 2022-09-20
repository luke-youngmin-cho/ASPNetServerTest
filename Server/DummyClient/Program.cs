using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;
namespace DummyClient
{
    class Packet
    {
        public ushort Size;
        public ushort Id;
    }

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            // 송신
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"OnConnected : {endPoint}");

                Packet packet = new Packet() { Size = 4, Id = 7 };

                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                byte[] buffer = BitConverter.GetBytes(packet.Size);
                byte[] buffer2 = BitConverter.GetBytes(packet.Id);
                Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
                Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
                ArraySegment<byte> sendBuffer = SendBufferHelper.Close(packet.Size);

                Send(sendBuffer);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override int OnReceive(ArraySegment<byte> buffer)
        {
            string receiveData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {receiveData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes {numOfBytes}");
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 입장 하려는 서버 말단 IP 

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); } );

            while (true)
            {
                try
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(1000);
            }
        }
    }
}
