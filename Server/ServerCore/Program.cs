using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {
        static void MainThread(object state)
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Hello Thread");
            }   
        }
        static void Main(string[] args)
        {

            // C# Thread는 기본적으로 forground 실행
            // forground 는 main thread 와 상관없이 종료될떄까지 실행됨
            // background 는 main thread 가 종료될때 같이 종료됨
            // Thread 를 많이 만든다고해서 효율이 비례하는것은 아님

            //Thread t = new Thread(MainThread);
            //t.Name = "Test Thread";
            //t.IsBackground = true;
            //t.Start();
            //Console.WriteLine("Wait for thread started");
            //t.Join();
            //Console.WriteLine("Hello world");

            // ThreadPooling
            // ThreadPool 을 사용할 떄는, 가급적 적은 연산을 수행하도록 하는것이 좋음
            // 과도한 연산이 부과되면 / 과도한 쓰레드를 사용하면 ThreadPool 이 중단될 수 있다.
            // 방지하기 위한 방법으로는 Min, Max thread 지정해주는방법과 Task를 이용하는 방법이 있다.
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);
            for (int i = 0; i < 5; i++)
                ThreadPool.QueueUserWorkItem((obj) => { while (true) { } });

            ThreadPool.QueueUserWorkItem(MainThread);

            // Task
            // Task 도 ThreadPool 을 사용하게됨
            // 연산량이 많다면 LongRunning 옵션 설정을 해 줄수있다.
            // LongRunning 옵션은 해당 Thread 를 별도로 관리하도록하고, ThreadPool 의 Min, Max 설정과 무관하게 관리된다.
            for (int i = 0; i < 5; i++)
            {
                Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
                t.Start();

            }

            while (true)
            {

            }
        }
    }
}
