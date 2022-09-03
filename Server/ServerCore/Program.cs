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
        static Listener _listener = new Listener();
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        static void Main(string[] args)
        {
            // DNS (Domain Name System)
            // www.luke.com -> 123.123.123.2 
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 7777); // 말단 IP 


            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");


            while (true)
            {
            }
        }
    }
}
