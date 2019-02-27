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
        /// </summary>
        public void ThreadDataAndLocal(){

        }

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
