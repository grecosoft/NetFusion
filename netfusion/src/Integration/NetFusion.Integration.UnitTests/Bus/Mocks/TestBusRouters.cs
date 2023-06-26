using NetFusion.Integration.Bus;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

/// <summary>
/// Router is defined for a given message broker (RabbitMQ, Azure Service Bus, Redis)
/// and specifies supported message patterns and to which entities (queues/topics) a
/// message is delivered when dispatched by the publishing microservice.  Also used
/// by the subscribing microservice to route received messages to the corresponding
/// message handler.
/// </summary>
public abstract class TestBusRouterBase : BusRouterBase
{
    protected TestBusRouterBase(string busName) : base(busName)
    {
    }
}

public class TestBusRouter : TestBusRouterBase
{
    public TestBusRouter() : base("testBusName")
    {
    }

    protected override void OnDefineEntities()
    {
        // In a non-test route, a fluent-api is provided used 
        // to create the corresponding BusEntities specifying
        // how messages are sent to Queues/Topics and to which
        // consumers received messages are dispatched.
        
        AddBusEntity(new TestQueueBusEntity(BusName, "TestQueue"));
        AddBusEntity(new TestSubscriptionBusEntity(BusName, "TestSubscription"));
    }
}

public class DuplicateBusRouter : TestBusRouterBase
{
    public DuplicateBusRouter() : base("testBusName")
    {
    }

    protected override void OnDefineEntities()
    {
       
    }
}

