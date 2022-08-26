using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {

        // 디버그모드에서는 정상작동 하지만
        // 릴리스모드에서는 컴파일러가 코드최적화를 하는 과정에서 동작이 달라짐 (volatile을 사용하지 않았을 경우)
        volatile static private bool _stop = false;
        static private void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작");

            // 디스어셈블리를 들여다보면 
            // 컴파일러가 컴파일을할 때 최적화하기 위해서 
            // 
            // if (_stop == false)
            // while (true) {} 
            // 
            // 형태로 컴파일을 하는데, 멀티쓰레딩으로 외부 스레드에서 while 조건을 변경하고싶은 경우 
            // 이런 최적화는 오작동을 일으킴. 
            // 이럴때 사용할 수 있는조건이 volatile 키워드.
            // volatile 키워드는 최적화를 하지 않도록하는 키워드
            // C#의 volatile은 C++ 의 volatile 과 의미가 달라서 사용하지 않는것을 권함. 
            // 그래서 lock 등으로 해결함
            while (_stop == false)
            {

            }

            Console.WriteLine("쓰레드 종료");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);

            t.Start();
            Thread.Sleep(1000);
            _stop = true;


            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");
            t.Wait();
            Console.WriteLine("종료 성공");
        }
    }
}
