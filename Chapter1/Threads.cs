using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chapter1;
using Xunit;

namespace Chapter1
{
    /// <summary>
    /// Threads are lower level abstraction than Tasks
    /// Task => Item of work
    /// Thread => Process running
    /// 
    /// Threads can be created as foreground processes  => app wont terminate with active foreground thread
    /// Tasks are created as background processesm they can be terminated before they complete,
    /// 
    /// Threads have a priority that can be changed during the lifetime of the thread
    /// Threads cannot deliver results to another thread, must comunicate by using shared variables, sync issues
    /// No Continuation support for threads, Join is available which allows to pause until another completes
    /// No aggregated exceptions over a number of threads, an exception thrown inside a thread must be dealt within th code in that thread
    /// </summary>
    public class Threads
    {
        [Fact]
        public void CreateThread()
        {
            var thread = new Thread(ThreadHello);
            thread.Start();
        }

        [Fact]
        public void ThreadLambdaExpression()
        {
            var thread = new Thread(() =>
            {
                Console.WriteLine("Hello from the thread");
            });
        }

        [Fact]
        /// <summary>
        /// Using ParameterizedThreadStart to pass data into a thread, single object paramter
        /// </summary>
        public void PassingDataToThread()
        {
            var threadStartWithParameter = new ParameterizedThreadStart(WorkOnData);
            var thread = new Thread(threadStartWithParameter);
            thread.Start(99);

            var threadWithLambda = new Thread((data) =>
            {
                WorkOnData(data);
            });
            threadWithLambda.Start(98);
        }

        [Fact]
        /// <summary>
        /// When a thread is aborted it is instantly stopped, leaving in an ambigious state, use cancelation token or shared variable
        /// </summary>
        public void AbortingThread()
        {

            CancellationTokenSource cts = new CancellationTokenSource();

            var tickThread = new Thread(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    Console.WriteLine("Tick");
                    Thread.Sleep(1000);
                }
            });

            tickThread.Start();
            Console.WriteLine("Thread Started");
            Thread.Sleep(5000);
            cts.Cancel();
            Console.WriteLine("Thread Aborted");
        }

        [Fact]
        /// <summary>
        /// Using Join to wait for thread to finish processing
        /// </summary>
        public void ThreadSyncronization()
        {
            var threadToWaitFor = new Thread(() =>
            {
                Console.WriteLine("Thread starting");
                Thread.Sleep(2000);
                Console.WriteLine("Thread done");
            });

            var mainThread = new Thread(() =>
            {
                Console.WriteLine("Main Thread starting");
                threadToWaitFor.Join();
                Console.WriteLine("Main Thread done");
            });

            threadToWaitFor.Start();
            mainThread.Start();

            mainThread.Join();
        }

        [Fact]
        /// <summary>
        /// ThreadStatic means each thread will have its onw copy of a variable
        /// To initialize local data at thread level use ThreadLocal<T>
        /// Expected results are the same for both threads as the random generator is local only
        /// </summary>
        public void ThreadLocal(){

            Thread t1 = new Thread(() => {
                for(int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"t1 : {RandomGenerator.Value.Next(10)}");
                    Thread.Sleep(500);
                }
            });

            Thread t2 = new Thread(() => {
                for(int i = 0; i < 5; i++)
                {
                    Console.WriteLine($"t2 : {RandomGenerator.Value.Next(10)}");
                    Thread.Sleep(500);
                };
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();
        }

        [Fact]
        /// <summary>
        /// Thread exposes range of context information
        /// </summary>
        public void ThreadExecutionContext(){
            
            var currentThread = Thread.CurrentThread;

            Console.WriteLine($"Name: {currentThread.Name}");
            Console.WriteLine($"Culture: {currentThread.CurrentCulture}");
            Console.WriteLine($"Priority: {currentThread.Priority}");
            Console.WriteLine($"Context: {currentThread.ExecutionContext}");
            Console.WriteLine($"IsBackground: {currentThread.IsBackground}");
            Console.WriteLine($"IsPool: {currentThread.IsThreadPoolThread}");
        }

        [Fact]
        /// <summary>
        /// Threads are managed objects, so threads are  detroyed
        /// Thread Pool is a collection of reusable threads that canbe requested and then return to the pool to be used
        /// QueueUserWorkItem allocates a thread to run the item of work => WaitCallback delegate
        /// Not all Threads will execute at the same time
        /// Others are qyeyed
        /// There is no priority management at ThreadPoool level => background prio
        /// Local state variables are not cleared when threadpool thread is reused => Avoid!
        /// </summary>
        public void ThreadPoolTest()
        {
            for(int i = 0; i < 50; i++)
            {
                int stateNumber = i;
                ThreadPool.QueueUserWorkItem(state => DoWork(stateNumber));
            }
        }

        private void DoWork(int stateNumber)
        {
            Console.WriteLine($"Doing work: {stateNumber}");
            Thread.Sleep(500);
            Console.WriteLine($"Work finished: {stateNumber}");

        }

        private ThreadLocal<Random> RandomGenerator = new ThreadLocal<Random>(() =>{

            return new Random(2);
        });

        private void WorkOnData(object data)
        {
            Console.WriteLine($"Working on {data}");
            Thread.Sleep(1000);
        }
        private void ThreadHello()
        {
            Console.WriteLine("Hello from the thread");
            Thread.Sleep(2000);
        }
    }
}
