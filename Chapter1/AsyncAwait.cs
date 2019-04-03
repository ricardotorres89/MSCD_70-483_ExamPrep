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
    /// Async methods tell the compiler to expect an await keyword
    /// await keyword represents a statement of intent and precedes a call to a method that will return a task
    /// </summary>
    public class AsyncAwait
    {
        [Fact]
        public async void AsyncSuccessfull()
        {
            var result = await FetchSomething("hello");
            Assert.Equal("hello", result);
        }

        [Fact]
        /// <summary>
        /// async void methods dont allow for catching exception
        /// </summary>
        /// <returns></returns>
        public async void AsyncException()
        {

            Exception ex = await Assert.ThrowsAnyAsync<Exception>(() => FetchException());
            Assert.Equal("Error while fetching", ex.Message);
        }

        [Fact]
        public async void AwaitParallelTasks()
        {
            var results = await FetchMultiple(new string []{"1", "2", "3"});

            Assert.Contains("1", results);
            Assert.Contains("2", results);
            Assert.Contains("3", results);
        }

        private async Task<IEnumerable<string>> FetchMultiple(string [] values){
            var tasks = new List<Task<String>>();

            foreach(var value in values)
            {
                tasks.Add(FetchSomething(value));
            }

            return await Task.WhenAll(tasks);
        }

        private async Task<string> FetchException(){
            await Task.Delay(500);
            throw new Exception("Error while fetching");
        }

        private async Task<string> FetchSomething(string value){
            await Task.Delay(500);
            return value;
        }

    }
}
