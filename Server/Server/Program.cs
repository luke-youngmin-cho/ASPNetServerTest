using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class Packet
    {
        public ushort Size;
        public ushort Id;
    }

    class GameSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            //Packet packet = new Packet() { Size = 100, Id = 10 };
            //
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);            
            //byte[] buffer = BitConverter.GetBytes(packet.Size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.Id);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuffer = SendBufferHelper.Close(buffer.Length + buffer2.Length);
            //
            //Send(sendBuffer);

            Thread.Sleep(5000);

            Disconnect();
        }
        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + PacketSession.HeaderSize);
            Console.WriteLine($"Received packet id : {id}, size : {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes {numOfBytes}");
        }
    }

    internal class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // www.luke.com -> 123.123.123.2 
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 말단 IP 


            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening...");


            while (true)
            {
            }
        }
    }
}
