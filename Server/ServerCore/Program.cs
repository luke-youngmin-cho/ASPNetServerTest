using System;
using System.Threading;
using System.Threading.Tasks;

// Race Condition Test
namespace ServerCore
{
    // Dead lock 예시
    // Dead lock 을 해결하기위한 이상적인 방법은 없음 .
    // 대부분은 설계를 바꾸던지, 탈출해야하는 조건을 상황마다 맞게 만들어줌

    // lock rapping
    // lock 걸때 id 를 부여하는 형태의 설계를 할 수 있음
    class FastLock
    {
        public int Id;
    }


    class SessionManager
    {
        static object _lock = new object();

        public static void TestSession()
        {
            lock (_lock)
            {

            }
        }

        public static void Test()
        {
            lock (_lock)
            {
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object _lock = new object();

        public static void Test()
        {
            lock (_lock)
            {
                SessionManager.TestSession();
            }
        }

        public static void TestUser()
        {
            lock (_lock)
            {
                
            }
        }
    }
    internal class Program
    {   
        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                SessionManager.Test();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                UserManager.Test();
            }
        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            //Thread.Sleep(100); // 약간의 시간차이만 있어도 데드록에 빠지지 않을수 있기 때문에, 사전에 데드록을 완전히 방지하기는 아주 힘들다.
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine("Finished");
        }
    }
}
