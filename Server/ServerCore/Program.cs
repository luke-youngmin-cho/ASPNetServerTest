using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;



        // 컴파일시
        // 변수들이 연관이 없다면 
        // 아래처럼 순서를 바꿔서 실행해 버릴 수 있음.
        // 그래서 원래라면 Main 함수의 While 문을 빠져나올수 없어야하지만 빠져나올수 있게됨.
        // 이렇게 컴파일러가 실행 순서를 바꿔버리는것을 막기위해서 
        // 메모리 배리어를 쓸 수 있음.
        //static void Thread_2()
        //{
        //    x = 1;
        //    r2 = y;
        //}
        // to ->
        //static void Thread_2()
        //{
        //    r2 = y;
        //    x = 1;
        //}

        // 메모리배리어
        // A. 코드 재배치 억제
        // B. 가시성

        // 1. Full Memory Barrier (ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘 다 막는다.
        // 2. Store Memory Barreir (ASM SFENCE) : Store만 막는다
        // 3. Load Memory Barrier (ASM LFENCE) : Load만 막는다.
        static void Thread_1()
        {
            y = 1;
            Thread.MemoryBarrier();
            r1 = x;
            Thread.MemoryBarrier();
        }

        static void Thread_2()
        {
            x = 1;
            Thread.MemoryBarrier();
            r2 = y;
            Thread.MemoryBarrier();
        }
        
        static void Thread_3()
        {
            Thread.MemoryBarrier();
            if (r1 == 0)
            {
                Thread.MemoryBarrier();
                Console.WriteLine($"r1 is 0");
            }
            else
            {
                Thread.MemoryBarrier();
                Console.WriteLine($"r1 is not 0");
            }
        }

        static void Main(string[] args)
        {
            int count = 0;
            while (true)
            {
                count++;
                x = y = r1 = r2 = 0;

                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);
                Task t3 = new Task(Thread_3);
                t1.Start();
                t2.Start();
                t3.Start();

                Task.WaitAll(t1, t2);

                if (r1 == 0 &&
                    r2 == 0)
                    break;
            }

            Console.WriteLine($"{count}번 만에 빠져나옴");
        }
    }
}
