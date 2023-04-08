using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Routing;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

public class TestSubscriptionBusEntity : BusEntity
{
    private readonly MessageDispatcher _dispatcher;
    
    public TestSubscriptionBusEntity(string busName, string entityName) 
        : base(busName, entityName)
    {
        // If a given bus entity models a subscription, it provides
        // the dispatcher to call when a message is received.
        var messageRouter = new CommandRouteWithMeta<TestCommand, TestQueueMeta<TestCommand>>();
        messageRouter.ToConsumer<TestCommandConsumer>(c => c.OnCommand, meta => {});
        _dispatcher = new MessageDispatcher(messageRouter);
        
        AddStrategies(new TestSubscriptionStrategy(this));
    }

    public override IEnumerable<MessageDispatcher> Dispatchers => new[] { _dispatcher };
}
public class TestQueueMeta
{
    public string? QueueName { get; set; }
    public string? DeadLetterExchangeName { get; set; }
}

public class TestQueueMeta<TMessage> : TestQueueMeta, IRouteMeta<TMessage>
    where TMessage : IMessage
{

}

public class TestSubscriptionStrategy : BusEntityStrategyBase<TestBusEntityContext>,
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    public TestSubscriptionStrategy(BusEntity busEntity) : base(busEntity)
    {
    }
    
    public TestBusEntityContext StrategyContext => Context;
    public bool CreationStrategyExecuted { get; private set; } 
    public bool SubscriptionStrategyExecuted { get; private set; } 
    public bool DisposalStrategyExecuted { get; private set; } 
    
    public Task CreateEntity()
    {
        CreationStrategyExecuted = true;
        return Task.CompletedTask;
    }

    public Task SubscribeEntity()
    {
        SubscriptionStrategyExecuted = true;
        return Task.CompletedTask;
    }

    public Task OnDispose()
    {
        DisposalStrategyExecuted = true;
        return Task.CompletedTask;
    }
}