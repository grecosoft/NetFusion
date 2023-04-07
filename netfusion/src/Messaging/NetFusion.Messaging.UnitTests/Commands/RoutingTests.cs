using FluentAssertions;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Messaging.UnitTests.Commands.Mocks;

namespace NetFusion.Messaging.UnitTests.Commands;

public class RoutingTests
{
    [Fact]
    public void WhenBuilt_MessageDispatcher_CreatedFromRoutes()
    {
        var router = new CommandRouter();
        router.SetupValidSyncCommandRoute();

        var dispatchers = router.BuildMessageDispatchers();
        dispatchers.Should().HaveCount(1, "each route creates a dispatcher");
    }

    [Fact]
    public void WhenBuilt_MessageDispatcher_HasMessageType()
    {
        var router = new CommandRouter();
        router.SetupValidSyncCommandRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().MessageType.Should().Be(typeof(CommandRouter.TestCommand), 
            "a created route dispatcher has associated message type");
    }
    
    [Fact]
    public void WhenBuilt_MessageDispatcher_HasConsumerType()
    {
        var router = new CommandRouter();
        router.SetupValidSyncCommandRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().ConsumerType.Should().Be(typeof(CommandRouter.TestCommandConsumer), 
            "a created route dispatcher has the consumer that will handel message");
    }

    [Fact]
    public void WhenBuilt_MessageDispatcher_HasHandlerMethodInfo()
    {
        var router = new CommandRouter();
        router.SetupValidSyncCommandRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        var expectedHandlerMethod = typeof(CommandRouter.TestCommandConsumer).GetMethod("HandleSync");
        expectedHandlerMethod.Should().NotBeNull();
        
        dispatchers.First().MessageHandlerMethod.Should().BeSameAs(expectedHandlerMethod, 
            "dispatcher contains method-info for consumer's message handler");
    }

    [Fact]
    public void CommandRouted_MessageDispatcher_SpecifiesSynchronousHandler()
    {
        var router = new CommandRouter();
        router.SetupValidSyncCommandRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsAsync.Should().BeFalse( 
            "commands can be sent to synchronous message handlers");
    }
    
    [Fact]
    public void CommandRouted_MessageDispatcher_SpecifiesAsyncHandler()
    {
        var router = new CommandRouter();
        router.SetupValidAsyncCommandRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsAsync.Should().BeTrue( 
            "commands can be sent to async message handlers");

        dispatchers.First().IsAsyncWithResult.Should().BeTrue(
            "an async handler can return a command's result");
    }
    
    [Fact]
    public void CommandRouted_MessageDispatcher_SpecifiesAsyncHandlerWithNoResult()
    {
        var router = new CommandRouter();
        router.SetupValidAsyncCommandWithNoResultRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsAsync.Should().BeTrue( 
            "commands can be send to async message handlers");

        dispatchers.First().IsAsyncWithResult.Should().BeFalse(
            "an async handler doesn't have to return a result");
    }
    
    [Fact]
    public void CommandRouted_MessageDispatcher_SpecifiesSyncHandlerWithNoResult()
    {
        var router = new CommandRouter();
        router.SetupValidSyncCommandWithNoResultRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsAsync.Should().BeFalse( 
            "commands can be send to synchronous message handlers");

        dispatchers.First().MessageHandlerMethod.ReturnType.Should().Be(typeof(void),
            "a synchronous handler doesn't have to return a result");
    }
    
    [Fact]
    public void CommandRouted_MessageDispatcher_SpecifiesIfAsyncHandlerCanBeCanceled()
    {
        var router = new CommandRouter();
        router.SetupValidAsyncCommandWithCancelRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsCancellable.Should().BeTrue(
            "a command's async handler can be canceled");
    }

    [Fact]
    public void CommandRouted_ToHandler_CanOnlyHaveOneConsumer()
    {
        var router = new CommandRouter();

        var exception = Assert.Throws<BootstrapException>(
            () => router.SetupCommandWithMultipleConsumerHandlers());

        exception.ExceptionId.Should().Be("message-already-routed", 
            "exception raised if command is route to more than one handler");
    }
}