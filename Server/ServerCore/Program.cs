using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // www.luke.com -> 123.123.123.2 
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 말단 IP 

            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(endPoint); // 소켓이 말단 IP 와 통신가능하도록 함

                listenSocket.Listen(10); // backlog : 최대 대기수

                while (true)
                {
                    Console.WriteLine("Listening...");
                    Socket clientSocket = listenSocket.Accept(); // 통신 연결한 클라이언트 소켓 반환

                    // 수신
                    byte[] recieveBuffer = new byte[1024];
                    int recievedBytes = clientSocket.Receive(recieveBuffer);
                    string recievedData = Encoding.UTF8.GetString(recieveBuffer, 0, recievedBytes);
                    Console.WriteLine($"[From Client] {recievedData}");

                    // 송신
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to server!");
                    clientSocket.Send(sendBuffer);

                    // 클라이언트 내보내기
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }            
        }
    }
}
