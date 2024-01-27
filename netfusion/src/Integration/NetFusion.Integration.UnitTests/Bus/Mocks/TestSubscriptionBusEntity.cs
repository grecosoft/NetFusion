using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Routing;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

public class TestSubscriptionBusEntity : BusEntity
{
    public List<string> InvokedStrategies { get; } = new();
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
    where TMessage : IMessage;

public class TestSubscriptionStrategy : BusEntityStrategyBase<TestBusEntityContext>,
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly TestSubscriptionBusEntity _entity;
    
    public TestSubscriptionStrategy(TestSubscriptionBusEntity busEntity) : base(busEntity)
    {
        _entity = busEntity;
    }
    
    public TestBusEntityContext StrategyContext => Context;
    public bool CreationStrategyExecuted => _entity.InvokedStrategies.Contains(nameof(CreateEntity));
    public bool SubscriptionStrategyExecuted => _entity.InvokedStrategies.Contains(nameof(SubscribeEntity));
    public bool DisposalStrategyExecuted => _entity.InvokedStrategies.Contains(nameof(OnDispose));
    
    public Task CreateEntity()
    {
        _entity.InvokedStrategies.Add(nameof(CreateEntity));
        return Task.CompletedTask;
    }

    public Task SubscribeEntity()
    {
        _entity.InvokedStrategies.Add(nameof(SubscribeEntity));
        return Task.CompletedTask;
    }

    public Task OnDispose()
    {
        _entity.InvokedStrategies.Add(nameof(OnDispose));
        return Task.CompletedTask;
    }
}