using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {
        // TLS ( Thread Local Storage )
        // 쓰레드들이 하나의 Lock 에 몰려서 연산이 지연되는 문제를 해결하기위해 
        // 쓰레드들을 일정량 묶어서 스택영역에 가져와서 분배 처리 하는 방법
        // MMORPG 같은경우에 많은 클라이언트가 게임로직에 대한 자원에 전부 접근하기떄문에 
        // TLS 가 해결방법으로 중요하게 사용된다.
        static ThreadLocal<string> ThreadLocal = new ThreadLocal<string>(() =>
        { 
            return $"My name is {Thread.CurrentThread.ManagedThreadId}";
        });
        static void WhoAmI()
        {
            bool repeat = ThreadLocal.IsValueCreated;
            if (repeat)
                Console.WriteLine(ThreadLocal.Value + "{repeat}");
            else
                Console.WriteLine(ThreadLocal.Value);
        }
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            // 인자로 넘겨준 함수를 스레드풀에서 가져온 각 스레드에 할당해서 호출해주는 함수
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            ThreadLocal.Dispose();
        }
    }
}
