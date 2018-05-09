using FluentAssertions;
using NetFusion.Common.Extensions.Tasks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CommonTests.Extensions.Tasks
{
    /// <summary>
    /// When executing a collection of tasks, representing asynchronous results, the exception for the first failing 
    /// Task is returned.  The following tests the TaskListItem and  the associated extensions.  The implementation 
    /// associates a Task with the Invoker that produced that Task.  This allows for easy logging of the Invoker 
    /// information that the failed Task is associated with.  A collection of these task items are maintained and 
    /// all failed task exceptions can be queried and logged.
    /// </summary>
    public class TaskExtensions
    {
        [Fact (DisplayName = "No Exceptions when All task list items Succeed")]
        public async Task NoExceptions_WhenAllTaskListItemsSucceed()
        {
            // State and list of invokers that all succeed.
            var state = new TestState { Value1 = 100, Value2 = 200 };
            var invokers = new[] {
                new TestInvoker(),
                new TestInvoker()
            };

            // Caller specifies via lambda what method on invoker is called and passes state.
            var taskList = invokers.Invoke(state, (i, s) => i.Invoke(s));
            await taskList.WhenAll();

            // The caller specifies the custom exception that should be created for each failed 
            // Task - for this test, there should be none.
            var exceptions = taskList.GetExceptions(
                    fr => new TestException("Expected Exception", fr.Task.Exception, fr.Invoker));

            exceptions.Should().BeEmpty();
        }

        [Fact(DisplayName = "Can Query Exceptions when one or more task list item Fails")]
        public async Task CanQueryException_WhenOneOrMoreTalskListItemFails()
        {
            var state = new TestState { Value1 = 100, Value2 = 200 };
            var invokers = new[] {
                new TestInvoker(),
                new TestInvoker("Invoker Error1"),
                new TestInvoker(),
                new TestInvoker("InvokerError2") };

            var taskList = invokers.Invoke(state, (i, s) => i.Invoke(s));

            try {
                await taskList.WhenAll();
                Assert.False(true, "Expected Exception Not Raised");
            }
            catch 
            {
                var exceptions = taskList.GetExceptions(
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
            ErrorMessage = errorMessage;
        }

        public Task Invoke(TestState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (ErrorMessage == null)
            {
                return Task.CompletedTask;
            }

            return Task.FromException(new InvalidOperationException(ErrorMessage));
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
            Invoker = invoker;
        }
    }
}
