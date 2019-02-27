using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chapter1;
using Xunit;

namespace Chapter1
{
    public class ParallelLinq
    {
        [Fact]
        /// <summary>
        /// Invoking multiple Actions in parallel
        /// </summary>
        public void RunTasksInParallel()
        {

            Parallel.Invoke(() => Task1(), () => Task2());
        }

        [Fact]
        /// <summary>
        /// Parallel for each on IEnumerable, tasks are not completed in the same order they start
        /// </summary>
        public void RunParallelForEach()
        {
            var items = Enumerable.Range(0, 500);
            Parallel.ForEach(items, item =>
            {
                WorkOnItem(item);
            });
        }

        [Fact]
        /// <summary>
        /// Parallel for loop usage with start/end index
        /// </summary>
        public void RunParallelFor()
        {

            var items = Enumerable.Range(0, 500).ToArray();

            Parallel.For(0, items.Length, i =>
            {
                WorkOnItem(items[i]);
            });
        }

        [Fact]
        /// <summary>
        /// Stops a parallel loop, does not ensure that lower index iterations are performed
        /// </summary>
        public void StopsParallelLoop()
        {
            var items = Enumerable.Range(0, 500).ToArray();

            var result = Parallel.For(0, items.Count(), (int i, ParallelLoopState loopState) =>
            {

                if (i == 200)
                {
                    loopState.Stop();
                }

                WorkOnItem(items[i]);
            });
        }

        [Fact]
        /// <summary>
        /// Breaks a parallel loop, guarantees that operations with lower index are performed
        /// </summary>
        public void BreakParallelLoop()
        {
            var items = Enumerable.Range(0, 500).ToArray();

            var result = Parallel.For(0, items.Count(), (int i, ParallelLoopState loopState) =>
            {

                if (i == 200)
                {
                    loopState.Break();
                }

                WorkOnItem(items[i]);
            });
        }

        private void Task1()
        {
            Console.WriteLine("Task 1 starting");
            Thread.Sleep(2000);
            Console.WriteLine("Task 1 ending");
        }

        private void Task2()
        {
            Console.WriteLine("Task 2 starting");
            Thread.Sleep(2000);
            Console.WriteLine("Task 2 ending");
        }

        private void WorkOnItem(object item)
        {
            Console.WriteLine($"Starting working on {item}");
            Thread.Sleep(1000);
            Console.WriteLine($"Finished working on {item}");
        }
    }
}
