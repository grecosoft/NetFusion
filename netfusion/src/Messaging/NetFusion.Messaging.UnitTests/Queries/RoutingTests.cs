using FluentAssertions;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Messaging.UnitTests.Queries.Mocks;

namespace NetFusion.Messaging.UnitTests.Queries;

public class RoutingTests
{
    [Fact]
    public void WhenBuilt_MessageDispatcher_CreatedFromRoutes()
    {
        var router = new QueryRouter();
        router.SetupValidSyncQueryRoute();

        var dispatchers = router.BuildMessageDispatchers();
        dispatchers.Should().HaveCount(1, "each route creates a dispatcher");
    }

    [Fact]
    public void WhenBuilt_MessageDispatcher_HasMessageType()
    {
        var router = new QueryRouter();
        router.SetupValidSyncQueryRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().MessageType.Should().Be(typeof(QueryRouter.TestQuery), 
            "a created route dispatcher has associated message type");
    }
    
    [Fact]
    public void WhenBuilt_MessageDispatcher_HasConsumerType()
    {
        var router = new QueryRouter();
        router.SetupValidSyncQueryRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().ConsumerType.Should().Be(typeof(QueryRouter.TestQueryConsumer), 
            "a created route dispatcher has the consumer that will handel message");
    }

    [Fact]
    public void WhenBuilt_MessageDispatcher_HasHandlerMethodInfo()
    {
        var router = new QueryRouter();
        router.SetupValidSyncQueryRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        var expectedHandlerMethod = typeof(QueryRouter.TestQueryConsumer).GetMethod("HandleSync");
        expectedHandlerMethod.Should().NotBeNull();
        
        dispatchers.First().MessageHandlerMethod.Should().BeSameAs(expectedHandlerMethod, 
            "dispatcher contains method-info for consumer's message handler");
    }

    [Fact]
    public void QueryRouted_MessageDispatcher_SpecifiesSynchronousHandler()
    {
        var router = new QueryRouter();
        router.SetupValidSyncQueryRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsAsync.Should().BeFalse( 
            "commands can be sent to synchronous message handlers");
    }
    
    [Fact]
    public void QueryRouted_MessageDispatcher_SpecifiesAsyncHandler()
    {
        var router = new QueryRouter();
        router.SetupValidAsyncQueryRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsAsync.Should().BeTrue( 
            "commands can be sent to async message handlers");

        dispatchers.First().IsAsyncWithResult.Should().BeTrue(
            "an async handler can return a command's result");
    }

    [Fact]
    public void QueryRouted_MessageDispatcher_SpecifiesIfAsyncHandlerCanBeCanceled()
    {
        var router = new QueryRouter();
        router.SetupValidAsyncQueryWithCancelRoute();

        var dispatchers = router.BuildMessageDispatchers().ToArray();
        dispatchers.First().IsCancellable.Should().BeTrue(
            "a command's async handler can be canceled");
    }

    [Fact]
    public void QueryRouted_ToHandler_CanOnlyHaveOneConsumer()
    {
        var router = new QueryRouter();

        var exception = Assert.Throws<BootstrapException>(
            () => router.SetupQueryWithMultipleConsumerHandlers());

        exception.ExceptionId.Should().Be("message-already-routed", 
            "exception raised if command is route to more than one handler");
    }
}