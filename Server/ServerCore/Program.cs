using System;
using System.Threading;
using System.Threading.Tasks;

// Auto reset event
//
// spin lock 보다 훨씬 느림 (커널 레벨에서 체크하기때문에)
// 일반적으로 사용하지 않음.

namespace ServerCore
{
    class MyLock
    {
        AutoResetEvent _available = new AutoResetEvent(true);

        public void Acquire()
        {
            _available.WaitOne(); // _available.Reset() 은 WaitOne 에서 양자적으로 실행하므로 별도호출 필요없음
            // AutoResetEvent 대신 ManualResetEvent 를 쓴다면 WaitOne() 후에 Reset() 호출해야함
        }

        public void Release()
        {
            _available.Set();
        }
    }

    internal class Program
    {
        static int _num = 0;
        static MyLock _lock = new MyLock();
        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
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
