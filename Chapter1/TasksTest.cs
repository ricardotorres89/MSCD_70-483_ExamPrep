using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chapter1;
using Xunit;

namespace Chapter1
{
    public class TaskTest
    {
        [Fact]
        /// <summary>
        /// Starts and waits for a Task
        /// </summary>
        public void StartTask()
        {
            var newTask = new Task(() => DoWork());
            newTask.Start();
            newTask.Wait();

            var anotherTask = new Task(() => DoWork());
            anotherTask.RunSynchronously();
        }

        [Fact]
        /// <summary>
        /// Returning a value from a task, using result not await
        /// </summary>
        public void ReturnValueFromTask()
        {
            Task<int> task = Task.Run(() =>
            {
                return CalculateResult();
            });

            Assert.Equal(task.Result, 99);
        }

        [Fact]
        /// <summary>
        /// Wait a set of tasks for completion using WaitAll
        /// </summary>
        public void WaitAllTasks()
        {
            Task[] tasks = new Task[10];

            for (var i = 0; i < 10; i++)
            {
                int taskNum = i; // Make local copy so the correct task number is passed into the lambda expression
                tasks[i] = Task.Run(() => DoWork(taskNum));
            }

            Task.WaitAll(tasks);

            Assert.All(tasks, task => Assert.Equal(true, task.IsCompleted));
        }

        [Fact]
        public void ContinuationTasks()
        {
            var helloWorldString = string.Empty;

            var helloTask = Task.Run(() =>
            {
                Thread.Sleep(1000);
                helloWorldString += "Hello";
            });

            var worldTask = helloTask.ContinueWith((previousTask) =>
            {
                Thread.Sleep(1000);
                helloWorldString += "World";
            });

            worldTask.Wait();

            Assert.Equal("HelloWorld", helloWorldString);
        }

        [Fact]
        /// <summary>
        /// Using continuation options to only run on succcess, failure state
        /// </summary>
        public void ContinuationOptionOnException()
        {
            var helloWorldString = string.Empty;


            var helloTask = Task.Run(() =>
            {
                Thread.Sleep(1000);
                helloWorldString += "Hello";
                throw new Exception("test");
            });

            var successTask = helloTask.ContinueWith((previousTask) =>
            {
                Thread.Sleep(1000);
                helloWorldString += "World";
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            var exceptionTask = helloTask.ContinueWith((previousTask) =>
            {
                Thread.Sleep(1000);
                helloWorldString += "Exception";
            }, TaskContinuationOptions.OnlyOnFaulted);

            try
            {
                exceptionTask.Wait();
            }
            finally
            {
                Assert.Equal("HelloException", helloWorldString);
            }
        }

        [Fact]
        /// <summary>
        /// Code running inside a parent Task can create other tasks 
        /// but those will execute independenly of the parent that created them (detached child tasks)
        /// However we can created attached child tasks by using Task options AttachedToParent
        /// DenyChildAttached is the oposite
        /// Task.Run uses DenyChildAttached
        /// </summary>
        public void AttachChildTasks()
        {
            var parent = Task.Factory.StartNew(() =>
            {

                Console.WriteLine("Parent starts");
                for (var i = 0; i < 10; i++)
                {
                    var taskNo = i;
                    var child = Task.Factory.StartNew(
                        (x) => DoChild(x), // lambda expression
                        taskNo,
                        TaskCreationOptions.AttachedToParent);
                }
            });

            parent.Wait();

            Console.WriteLine("Parent finished");
        }

        [Fact]
        /// <summary>
        /// A thread can be aborted but a task must monitor a cancelation token
        /// </summary>
        /// <returns></returns>
        public async void CancelLongRunningTask()
        {
            var cancelationToken = new CancellationTokenSource();

            Task.Run(()=> Clock(cancelationToken));
            await Task.Delay(2000);
            cancelationToken.Cancel();
            Console.WriteLine("Clock cancelled");
        }

        [Fact]
        public async void CancelTaskWithException()
        {
            var cancelationTokenSource = new CancellationTokenSource();

            var clock = Task.Run(()=> ClockWithException(cancelationTokenSource.Token));

            await Task.Delay(3000);

            if(clock.IsCompleted)
            {
                Console.WriteLine("Clock task completed");
            }
            else
            {
                try
                {
                    cancelationTokenSource.Cancel();
                    clock.Wait();
                }
                catch(AggregateException ex)
                {
                    Console.WriteLine($"Clock stopped: {ex.InnerExceptions[0].ToString()}");
                }
            }
        }

        private void Clock(CancellationTokenSource cancelationTokenSource)
        {
            while(!cancelationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("Tick");
                Thread.Sleep(500);
            }
        }

        private void ClockWithException(CancellationToken cancellationToken)
        {
            int tickCount = 0;
            while(!cancellationToken.IsCancellationRequested && tickCount < 20)
            {
                tickCount++;
                Console.WriteLine("Tick");
                Thread.Sleep(500);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
        private void DoChild(object state)
        {
            Console.WriteLine($"Child {state} starting");
            Thread.Sleep(2000);
            Console.WriteLine($"Child {state} finished");
        }

        private void DoWork(int taskNum)
        {
            Console.WriteLine($"Work starting for Task {taskNum}");
            Thread.Sleep(2000);
            Console.WriteLine($"Work finished for Task {taskNum}");
        }

        private void DoWork()
        {
            Console.WriteLine("Work starting");
            Thread.Sleep(2000);
            Console.WriteLine("Work finished");
        }

        private int CalculateResult()
        {
            Console.WriteLine("Work starting");
            Thread.Sleep(2000);
            var value = 99;
            Console.WriteLine("Work finished");
            return value;
        }
    }
}
