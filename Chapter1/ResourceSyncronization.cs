using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chapter1;
using Xunit;

namespace Chapter1
{
    /// <summary>
    /// 
    /// </summary>
    public class ResourceSyncronization
    {
        private long _total;
        private int[] _items;
        private object _totalLock = new object();


        public ResourceSyncronization(){
            
            _total = 0;
            _items = Enumerable.Range(0, 50000001).ToArray();
        }

        [Fact]
        public void SummingBehaviour()
        {
           for(int i = 0; i < _items.Length; i++)
           {
               _total += _items[i];
           }

           Assert.Equal(1250000025000000, _total);
        }

        [Fact]
        /// <summary>
        /// Sum fails do to a race condition on the _total variable
        /// </summary>
        public void BadTaskSumming(){
            PerformSumWithTasks(AddRangeValues);
            Assert.NotEqual(1250000025000000, _total);
        }

        [Fact]
        /// <summary>
        /// Same code as before but with lock in place
        /// It is not a good idea to use a string as a lock as because .Net string implementation
        /// uses a pool of strings during compilation which can lead to the program using a reference
        /// to the same text when two different tasks use the same string text lock
        /// </summary>
        public void TaskSummingWithLocks(){
            PerformSumWithTasks(AddRangeValuesWithLock);
            Assert.Equal(1250000025000000, _total);
        }

        [Fact]
        /// <summary>
        /// Monitor provides similar set of actions to a lock
        /// They allow for one thread to access a particular object
        /// Instead of controlling a statement of block of code as the lock keyword
        /// the atomic code is enclosed in calls Monitor.Enter/Exit
        /// Monitors allow to check if it the lock variable is currently locked with TryEnter
        /// </summary>
        public void UsingMonitors()
        {
            PerformSumWithTasks(AddRangeValuesUsingMonitor);
            Assert.Equal(1250000025000000, _total);
        }


        
        [Fact]
        /// <summary>
        /// Iterlocked class provides set of thread sage operations on a variable
        /// increment,decrement, exchange etc
        /// </summary>
        public void InterlockedOperations(){
            PerformSumWithTasks(AddRangeValuesUsingInterlocked);
            Assert.Equal(1250000025000000, _total);
        }

        [Fact]
        /// <summary>
        /// Acquiring locks sequential in a non multi threaded environrment works fine
        /// </summary>
        public void SequentialLocking(){
            var lock1 = new object();
            var lock2 = new object();

            AcquireLocks("call1", lock1, lock2);
            AcquireLocks("call2", lock2, lock1);            
        }

        [Fact]
        /// <summary>
        /// Ilustrates the problem with nested locks 
        /// </summary>
        public void DeadlockedTasks(){
            var lock1 = new object();
            var lock2 = new object();

            var t1 = Task.Run(() => AcquireLocks("task1", lock1, lock2));
            var t2 = Task.Run(() => AcquireLocks("task2", lock2, lock1));

            Console.WriteLine("Waiting for task2");
            t2.Wait();
        }

        private void AcquireLocks(string owner, object lockObject1, object lockObject2)
        {
            lock(lockObject1)
            {
                Console.WriteLine($"{owner} got {nameof(lockObject1)}");
                Console.WriteLine($"{owner} waiting for {nameof(lockObject2)}");
                lock(lockObject2)
                {
                    Console.WriteLine($"{owner} got {nameof(lockObject2)}");
                }
                Console.WriteLine($"{owner} released {nameof(lockObject2)}");
            }
            Console.WriteLine($"{owner} released {nameof(lockObject1)}");
        }

        private void PerformSumWithTasks(Action<int,int> AddRangeFunc)
        {
            var tasks = new List<Task>();

            int rangeSize = 1000;
            int rangeStart = 0;

            while(rangeStart < _items.Length)
            {
                int rangeEnd = rangeStart + rangeSize;

                if(rangeEnd > _items.Length)
                {
                    rangeEnd = _items.Length;
                }

                int rs = rangeStart;
                int re = rangeEnd;

                tasks.Add(Task.Run(()=> AddRangeFunc(rs,re)));
                rangeStart = rangeEnd;
            }

            Task.WaitAll(tasks.ToArray());
        }
        private void AddRangeValues(int start, int end)
        {
            while(start < end)
            {
                _total = _total + _items[start];
                start++;
            }
        }

        private void AddRangeValuesWithLock(int start, int end)
        {
            while(start < end)
            {
                lock(_totalLock)
                {
                    _total = _total + _items[start];
                    start++;
                }
            }
        }

        private void AddRangeValuesUsingMonitor(int start, int end)
        {
            long subTotal = 0;

            while(start < end)
            {
                subTotal = subTotal + _items[start];
                start++;
            }


            Monitor.Enter(_totalLock);
            try{
                _total = _total + subTotal;
                // Other code that might throw an Exception
            }
            finally
            {
                Monitor.Exit(_totalLock);
            }           
        }

        private void AddRangeValuesUsingInterlocked(int start, int end)
        {
            long subTotal = 0;

            while(start < end)
            {
                subTotal = subTotal + _items[start];
                start++;
            }

            Interlocked.Add(ref _total, subTotal);
        }
        

    }
}
