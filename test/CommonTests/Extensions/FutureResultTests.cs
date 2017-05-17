using FluentAssertions;
using NetFusion.Common;
using NetFusion.Common.Extensions.Tasks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CommonTests.Extensions
{
    /// <summary>
    /// When executing a collection of tasks, representing future results, the exception
    /// for the first failing Task is returned.  The following tests the FutureResult and 
    /// the associated extensions.  The implementation associates a Task with the Invoker
    /// that produced that Task.  This allows for easy logging of the Invoker information
    /// that the failed Task is associated with.  A collection of these future results are
    /// maintained and all failed task exceptions can be queried and logged.
    /// </summary>
    public class FutureResultTests
    {
        [Fact]
        public async Task CallInvokersAgainstInput_Succeeds_FutureResultNoExceptions()
        {
            // State and list of invokers that all succeed.
            var state = new TestState { Value1 = 100, Value2 = 200 };
            var invokers = new TestInvoker[] {
                new TestInvoker(),
                new TestInvoker() };

            // Caller specifies via lambda what method on invoker is called and passes state.
            var futureResults = invokers.Invoke(state, (i, s) => i.Invoke(s));
            await futureResults.WhenAll();

            // The caller specifies the custom exception that should be created for each failed 
            // Task - for this test, there should be none.
            var exceptions = futureResults.GetExceptions(
                    fr => new TestException("Expected Exception", fr.Task.Exception, fr.Invoker));

            exceptions.Should().BeEmpty();
        }

        [Fact]
        public async Task CallInvokersAgainstInput_Exceptions_CanGetExceptionAndInvoker()
        {
            var state = new TestState { Value1 = 100, Value2 = 200 };
            var invokers = new TestInvoker[] {
                new TestInvoker(),
                new TestInvoker("Invoker Error1"),
                new TestInvoker(),
                new TestInvoker("InvokerError2") };

            var futureResults = invokers.Invoke(state, (i, s) => i.Invoke(s));

            try {
                await futureResults.WhenAll();
                Assert.False(true, "Expected Exception Not Raised");
            }
            catch 
            {
                var exceptions = futureResults.GetExceptions(
                    fr => new TestException("Expected Exception", fr.Task.Exception, fr.Invoker));

                exceptions.Should().NotBeNull();
                exceptions.Should().HaveCount(2);

                var firstEx = exceptions.ElementAt(0);
                firstEx.Invoker.Should().Be(invokers.ElementAt(1));

                var secondEx = exceptions.ElementAt(1);
                secondEx.Invoker.Should().Be(invokers.ElementAt(3));
            }
        }
    }

    class TestInvoker
    {
        public string ErrorMessage { get; }

        public TestInvoker(string errorMessage = null)
        {
            this.ErrorMessage = errorMessage;
        }

        public Task Invoke(TestState state)
        {
            Check.NotNull(state, nameof(state));
            if (this.ErrorMessage == null)
            {
                return Task.CompletedTask;
            }

            return Task.FromException(new InvalidOperationException(this.ErrorMessage));
        }
    }

    class TestState
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }

    class TestException : Exception
    {
        public TestInvoker Invoker { get; }

        public TestException(string message, Exception innerException, TestInvoker invoker) :
            base(message, innerException)
        {
            this.Invoker = invoker;
        }
    }
}
