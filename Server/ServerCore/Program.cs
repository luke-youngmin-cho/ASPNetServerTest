using System;
using System.Threading;
using System.Threading.Tasks;

// Race Condition Test
namespace ServerCore
{
    internal class Program
    {   
        static int number = 0;

        static void Thread_1()
        {
            //for (int i = 0; i < 10000; i++)
            //    number++;

            // 어셈블리로 변환될떄 실제과정은
            // 세단계로 덧셈이 이루어짐. 
            for (int i = 0; i < 10000; i++)
            {
                /*   
                int temp = number;
                temp += 1; // <- 여기까지 실행하고 중간에 멈추는 일이 생기면 아이템은 획득했는데 유저 템창에 뜨지않는 등의 버그가 생길 수 있음.
                // 그래서 특히 멀티스레딩에서는 원자성 보장이 중요함 (쪼개질수 없는 단위의 연산)                
                number = temp; // <- Thread_2 에서 number 연산 중일때 쓰게되면 해당 연산이 무시될 수 있음. (Race condition)
                */

                // 원자성 보장 동작을 하고 싶으면 Interlock 을 사용할 수 있음. 
                Interlocked.Increment(ref number);
            }
        }

        static void Thread_2()
        {
            //for (int i = 0; i < 10000; i++)
            //    number--;

            for (int i = 0; i < 10000; i++)
            {
                int temp = number;
                temp -= 1;
                number = temp;
            }
        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(number);
        }
    }
}
