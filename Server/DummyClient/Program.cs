using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 입장 하려는 서버 말단 IP 

            while (true)
            {
                // 서버 통신용 소켓 생성
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected to {socket.RemoteEndPoint}"); // 연결된 서버 말단 IP

                    // 송신
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("Hello World");
                    int sendBytes = socket.Send(sendBuffer);

                    // 수신
                    byte[] receivedBuffer = new byte[1024];
                    int receivedByte = socket.Receive(receivedBuffer);
                    string receivedData = Encoding.UTF8.GetString(receivedBuffer, 0, receivedByte);
                    Console.WriteLine($"[From Sever] {receivedData}");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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
