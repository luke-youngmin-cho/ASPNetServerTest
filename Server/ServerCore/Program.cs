using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {
        static object _lock = new object();
        static SpinLock _lock2 = new SpinLock();
        static ReaderWriterLockSlim _lock3 = new ReaderWriterLockSlim();
        static bool _lockTaken;
        class Reward
        {
        }

        static Reward GetRewardById(int id)
        {
            _lock3.EnterReadLock();
            _lock3.ExitReadLock();
            return null;
        }

        static void AddReward(Reward reward)
        {
            // lock
            lock (_lock)
            {

            }

            // spin lock
            try
            {
                _lock2.Enter(ref _lockTaken);
            }
            finally
            {
                if (_lockTaken)
                    _lock2.Exit();
            }

            // reader writer lock
            _lock3.EnterWriteLock();
            _lock3.ExitWriteLock();
        }

        static void Main(string[] args)
        {
        }
    }
}
