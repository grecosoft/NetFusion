using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NetFusion.Common.Extensions.Tasks;

namespace NetFusion.Common.UnitTests.Extensions.Tasks;

/// <summary>
/// When executing a collection of tasks, representing asynchronous results, the exception for the first failing 
/// Task is returned.  The following tests the TaskListItem and  the associated extensions.  The implementation 
/// associates a Task with the Invoker that produced that Task.  This allows for easy logging of the Invoker 
/// information that the failed Task is associated with.  A collection of these task items are maintained and 
/// all failed task exceptions can be queried and logged.
/// </summary>
public class TaskExtensions
{
    [Fact]
    public async Task NoExceptions_WhenAllTaskListItems_Succeed()
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

    [Fact]
    public async void TaskList_CanBeCanceled()
    {
        var state = new TestState { Value1 = 5 };
        var invokers = new[] {
            new DelayedTestInvoker(),
            new DelayedTestInvoker()
        };

        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        // Caller specifies via lambda what method on invoker is called and passes state.
        var taskList = invokers.Invoke(state, (i, s, ct) => i.DoWork(s.Value1, ct), cts.Token);

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await taskList.WhenAll();
            taskList.All(ti => ti.Task.IsCanceled).Should().BeTrue();
        });
    }

    [Fact]
    public async Task CanQueryException_WhenOneOrMoreTaskListItemFails()
    {
        var state = new TestState { Value1 = 100, Value2 = 200 };
        var invokers = new[] {
            new TestInvoker(),
            new TestInvoker("Invoker Error1"),
            new TestInvoker(),
            new TestInvoker("Invoker Error2") };

        var taskList = invokers.Invoke(state, (i, s) => i.Invoke(s));

        try {
            await taskList.WhenAll();
            Assert.Fail("Expected Exception Not Raised");
        }
        catch 
        {
            var exceptions = taskList.GetExceptions(
                fr => new TestException("Expected Exception", fr.Task.Exception, fr.Invoker));

            exceptions.Should().NotBeNull();
            exceptions.Should().HaveCount(2);

            var firstEx = exceptions.ElementAt(0);
            firstEx.Invoker.Should().Be(invokers.ElementAt(1));
            firstEx.InnerException?.Message.Should().Be("Invoker Error1");

            var secondEx = exceptions.ElementAt(1);
            secondEx.Invoker.Should().Be(invokers.ElementAt(3));
            secondEx.InnerException?.Message.Should().Be("Invoker Error2");
        }
    }
        
    private class TestInvoker
    {
        public string ErrorMessage { get; }

        public TestInvoker(string errorMessage = null)
        {
            ErrorMessage = errorMessage;
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
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

    private class TestState
    {
        public int Value1 { get; init; }
        public int Value2 { get; init; }
    }

    private class TestException : Exception
    {
        public TestInvoker Invoker { get; }

        public TestException(string message, Exception taskException, TestInvoker invoker) :
            base(message, taskException.InnerException ?? taskException)
        {
            Invoker = invoker;
        }
    }

    private class DelayedTestInvoker
    {
        public Task DoWork(int seconds, CancellationToken token) 
            => Task.Delay(TimeSpan.FromSeconds(seconds), token);
    }
}