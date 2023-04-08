using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

public class TestQueueBusEntity : BusEntity
{
    public TestQueueBusEntity(string busName, string entityName) 
        : base(busName, entityName)
    {
        AddStrategies(new TestQueueStrategy(this));

    }
}

public class TestQueueStrategy : BusEntityStrategyBase<TestBusEntityContext>,
    IBusEntityCreationStrategy,
    IBusEntityPublishStrategy
{
    public TestQueueStrategy(BusEntity busEntity) : base(busEntity)
    {
    }

    public TestBusEntityContext StrategyContext => Context;
    public bool CreationStrategyExecuted { get; private set; } 
    public bool PublishStrategyExecuted { get; private set; } 
    
    public Task CreateEntity()
    {
        CreationStrategyExecuted = true;
        return Task.CompletedTask;
    }

    public bool CanPublishMessageType(Type messageType) => true;

    public Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        PublishStrategyExecuted = true;
        return Task.CompletedTask;
    }
}

