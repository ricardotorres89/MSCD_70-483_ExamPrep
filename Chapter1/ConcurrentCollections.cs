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
    /// Thread safe collections 
    /// Producers and consumer tasks acessing the same data structures
    /// </summary>
    public class ConcurrentCollections
    {
        [Fact]
        /// <summary>
        /// Using blocking collection in producer consumer scenario
        /// </summary>
        public void BlockingCollection()
        {
            var data = new BlockingCollection<int>(5); // Hold 5 items max

            // Producer
            Task.Run(() =>{
                // Attempts to add 10 items
                for(int i = 0; i < 10; i++)
                {
                    data.Add(i);
                    Console.WriteLine($"Data {i} added successfully");
                }

                // No more data to add
                data.CompleteAdding();
            });

            Console.WriteLine("Reading collection");

            // Consumer
            Task.Run(() =>{
                while(!data.IsCompleted)
                {
                    try{
                        int v = data.Take();
                        Console.WriteLine($"Data {v} taken successfully");
                    }
                    catch(InvalidOperationException){
                        // Exception can be thrown when the completed flag changes after while loop check
                    } 
                }
            });
        }

        [Fact]
        /// <summary>
        /// Encapsulating Concurrent Stack with Blocking Collection
        /// Results will be taken in LIFO
        /// </summary>
        public void BlockingCollectionWrapper()
        {
            
            var stackData = new BlockingCollection<int>(new ConcurrentStack<int>(), 5); // Hold 5 items max

            // Producer
            Task.Run(() =>{
                // Attempts to add 10 items
                for(int i = 0; i < 10; i++)
                {
                    stackData.Add(i);
                    Console.WriteLine($"Data {i} added successfully");
                }

                // No more data to add
                stackData.CompleteAdding();
            });

            Console.WriteLine("Reading collection");

            // Consumer
            Task.Run(() =>{
                while(!stackData.IsCompleted)
                {
                    try{
                        int v = stackData.Take();
                        Console.WriteLine($"Data {v} taken successfully");
                    }
                    catch(InvalidOperationException){
                        // Exception can be thrown when the completed flag changes after while loop check
                    } 
                }
            });
        }

        [Fact]
        /// <summary>
        /// Concurrent Queue, FIFO
        /// </summary>
        public void ConcurrentQueue()
        {
            var concurrentQueue = new ConcurrentQueue<string>();

            concurrentQueue.Enqueue("Ricardo");
            concurrentQueue.Enqueue("Torres");

            if(concurrentQueue.TryPeek(out string peekName))
            {
                Assert.Equal("Ricardo", peekName);
            }

            if(concurrentQueue.TryDequeue(out string dequeueName))
            {
                Assert.Equal("Ricardo", dequeueName);
            }

            if(concurrentQueue.TryPeek(out string lastName))
            {
                Assert.Equal("Torres", lastName);
            }
        }

        [Fact]
        /// <summary>
        /// Concurrent bag, order is not important
        /// </summary>
        public void ConcurrentBag(){
            var concurrentBag = new ConcurrentBag<string>();
            concurrentBag.Add("Jose");
            concurrentBag.Add("Ricardo");
            concurrentBag.Add("Torres");

            if(concurrentBag.TryPeek(out string name))
            {
                Assert.NotEqual(string.Empty, name);
            }
        }

        [Fact]
        public void ConcurrentDictionary(){
            var ages = new ConcurrentDictionary<string,int>();

            if(ages.TryAdd("Ricardo", 29))
            {
                Assert.Equal(29, ages["Ricardo"]);
            }

            if(ages.TryUpdate("Ricardo", 30, 29))
            {
                Assert.Equal(30, ages["Ricardo"]);
            }

            ages.AddOrUpdate("Ricardo", 1, (name,age) => age = age +1);
            Assert.Equal(31, ages["Ricardo"]);
        }

    }
}
