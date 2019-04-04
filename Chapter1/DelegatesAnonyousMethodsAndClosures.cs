using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Chapter1;
using Xunit;

namespace Chapter1
{
    /// <summary>
    /// Appart from Action and EventHandler types which provide predefined delegates we can create our own
    /// delegate vs Delegate
    /// lowercase => tells compiler to create a delegate type
    /// uppercase => Once lowecase was used to define a delegate type the custom delegate is realized as a Delegate class instance.
    /// </summary>
    public class DelegatesAnonyousMethodsAndClosures
    {
        
        delegate int IntOperation(int a, int b);

        private int Add(int a, int b)
        {
            return a + b;
        }

        private int Subtract(int a, int b)
        {
            return a - b;
        }

        [Fact]
        public void CustomDelegate()
        {
            var op = new IntOperation(Add);

            var addResult = op(2,2);
            Assert.Equal(4, addResult);

            op = Subtract;
            Assert.Equal(0, op(2,2));
        }

        [Fact]
        public void LambdaDelegate(){
            IntOperation add = (a,b) => a + b;
            IntOperation divide = (a,b) => {
                if(b == 0)
                {
                    throw new ArgumentException(nameof(b));
                }
                return a / b;
            };

            var addResult = add(2,2);
            Assert.Equal(4, addResult);

            var divideResult = divide(4,2);
            Assert.Equal(2, divideResult);
        }


        delegate int GetValue();
        GetValue getLocalInt;

        private void SetLocalInt(){
            var localInt = 99;

            getLocalInt = () => localInt;
        }

        [Fact]
        /// <summary>
        /// The extension a variable life used in a lambda expression
        /// </summary>
        public void VariableClosure()
        {
            SetLocalInt();
            var localInt = getLocalInt();
            Assert.Equal(99, localInt);
        }

        [Fact]
        public void UsingBuiltInDelegates()
        {
            Func<int,int,int> add = (a,b) => a + b; // Last type is the result type
            Action<string> print = (name) => Console.WriteLine(name); // no return value
            Predicate<int> isEven = (i) => i % 2 == 0; // boolean return value

            Assert.Equal(5, add(2,3));
            Assert.Equal(false, isEven(5));
            Assert.Equal(true, isEven(4));
        }

        [Fact]
        public async void UsingAnonymousMethods()
        {
            var i = 0;

            await Task.Run(() => 
            { // Anonymous method
                i = 10;
            });
            
            Assert.Equal(10,i);
        }
    }
}
