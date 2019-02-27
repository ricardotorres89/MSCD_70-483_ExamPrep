using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chapter1;
using Xunit;

namespace Chapter1
{
    public class ParallelLinqTest
    {
        class Person
        {
            public string Name { get;set; }
            public string City { get; set; }
        }

        private IEnumerable<Person> _people;

        public ParallelLinqTest(){
            _people = new List<Person>(){
                new Person { Name = "Person 8", City = "City 1"},
                new Person { Name = "Person 2", City = "City 2"},
                new Person { Name = "Person 5", City = "City 1"},
                new Person { Name = "Person 3", City = "City 1"},
                new Person { Name = "Person 4", City = "City 4"},
                new Person { Name = "Person 7", City = "City 2"},
                new Person { Name = "Person 1", City = "City 1"},
                new Person { Name = "Person 6", City = "City 1"},
            };
        }

        [Fact]
        /// <summary>
        /// As Parallel examines linq query if a parallel version would speed it up,
        /// If so the query is broken into processes and run concurrently
        /// </summary>
        public void SelectAsParallel(){
            var peopleFromCity1 = 
                from person in _people.AsParallel() 
                where person.City.Equals("City 1") 
                select person;
            
            foreach(var person in peopleFromCity1)
            {
                Console.WriteLine(person.Name);
            }
        }

        [Fact]
        /// <summary>
        /// AsParallel can be configured to run on a number of of processors, the results wont be ordered as per the collection
        /// </summary>
        public void ParallelizationParameters(){
            var peopleFromCity1 = _people
                                    .AsParallel()
                                    .WithDegreeOfParallelism(4)
                                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                    .Where(p => p.City.Equals("City 1"));

            foreach(var person in peopleFromCity1)
            {
                Console.WriteLine(person.Name);
            }
        }

        [Fact]
        /// <summary>
        /// AsParallel Order preserved, using AsOrdered
        /// Can slow down the query
        /// Doesnt prevent parallelization but organizes results afterwards
        /// </summary>
        public void OrderedParallelization(){
            var peopleFromCity1 = _people
                                    .AsParallel()
                                    .AsOrdered()
                                    .WithDegreeOfParallelism(4)
                                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                                    .Where(p => p.City.Equals("City 1"));

            foreach(var person in peopleFromCity1)
            {
                Console.WriteLine(person.Name);
            }
        }

        [Fact]
        /// <summary>
        /// AsSequential can be used to prevent ordering from being removed on a complex query
        /// </summary>
        public void SequentialParallelQuery(){
            var firstFourPeopleFromCity1 = 
                (
                    from person in _people.AsParallel()
                    where person.City.Equals("City 1")
                    orderby (person.Name)
                    select new {
                        Name = person.Name
                    }
                ).AsSequential().Take(4);
                
            foreach(var person in firstFourPeopleFromCity1)
            {
                Console.WriteLine(person.Name);
            }
        }

        [Fact]
        /// <summary>
        /// ForAll can be used to iterate through all elements of a query in parallel even before the query is complete
        /// Original order is not preserved
        /// </summary>
        public void IterateUsingForAll()
        {
            var peopleFromCity1 = _people.AsParallel().Where(p => p.City.Equals("City 1"));

            peopleFromCity1.ForAll( p => Console.WriteLine(p.Name));
        }

        [Fact]
        /// <summary>
        /// Aggregate exceptions caught when executing parallel operations
        /// </summary>
        public void ExceptionsInQueries(){
            try
            {
                var results = _people
                                .AsParallel()
                                .Where(p => CheckCity(p.City));
                results.ForAll(p => Console.WriteLine(p.Name));
            }
            catch(AggregateException e)
            {
                Console.WriteLine($"{e.InnerExceptions.Count} Exceptions");
            }
        }

        private bool CheckCity(string name)
        {
            if(!name.Equals("City 1"))
            {
                throw new ArgumentException(name);
            }
            return true;
        }
    }
}
