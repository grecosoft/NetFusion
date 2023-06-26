using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

public class TestQueueBusEntity : BusEntity
{
    public List<string> InvokedStrategies { get; } = new();


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
    private readonly TestQueueBusEntity _entity;
    
    public TestQueueStrategy(TestQueueBusEntity busEntity) : base(busEntity)
    {
        _entity = busEntity;
    }

    public TestBusEntityContext StrategyContext => Context;
    public bool CreationStrategyExecuted => _entity.InvokedStrategies.Contains(nameof(CreateEntity));
    public bool PublishStrategyExecuted  => _entity.InvokedStrategies.Contains(nameof(SendToEntityAsync));
    
    public Task CreateEntity()
    {
        _entity.InvokedStrategies.Add(nameof(CreateEntity));
        return Task.CompletedTask;
    }

    public bool CanPublishMessageType(Type messageType) => messageType == typeof(TestCommand);

    public Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        _entity.InvokedStrategies.Add(nameof(SendToEntityAsync));
        return Task.CompletedTask;
    }
}

