using System;
using System.Threading;
using System.Threading.Tasks;

// Spin lock
namespace ServerCore
{
    struct MySpinLock
    {
        volatile int _locked;

        public void Acquire()
        {
            // CompareExchange 구현
            while (true)
            {
                int expected = 0;
                int desired = 1;
                int original = Interlocked.CompareExchange(ref _locked, desired, expected);
                if (original == 0)
                    break;
            }

            //Thread.Sleep(1); // 무조건 휴식, 1 ms 정도 대기 ( 운영체제의 스케쥴러가 시간 결정하므로 정확하지는 않음)
            //Thread.Sleep(0); // 쓰레드 조건 양보, 현재 쓰레드보다 우선순위가 낮은 쓰레드에게는 스케쥴 양보 안하고 바로 실행함. 같거나 높으면 다른 쓰레드 먼저 스케쥴링함.
            Thread.Yield(); // 쓰레드 무조건 양보, 실행 가능한 쓰레드가 있으면 바로 양도, 없으면 현재 쓰레드가 다시실행됨.

            // Exchange 구현
            //while (true)
            //{
            //    // 1 을 _locked 에 넣음으로서 락을 거는것을 시도하는데, 
            //    // original 이 0이 반환되어야 원래 0에서 1로 잠금에 성공한 것이고
            //    // 잠금에 성공한 쓰레드가 해당 자원을 점유하도록 해 주는 원리
            //    int original = Interlocked.Exchange(ref _locked, 1);
            //    if (original == 0)
            //        break;
            //}

            // 아래형태로 쓰면 while 조건을 거의동시에 여러쓰레드가 실행할 경우 레이스컨디션이 발생하게된다. (원자성 동작이 아님)
            /*
            while (_locked)
            {
                // 잠금 풀림 대기
            }

            // 점유
            _locked = 1;
            */
        }

        public void Release()
        {
            _locked = 0;
        }
    }

    internal class Program
    {
        static int _num = 0;
        static MySpinLock _lock = new MySpinLock();
        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(_num);
        }
    }
}
