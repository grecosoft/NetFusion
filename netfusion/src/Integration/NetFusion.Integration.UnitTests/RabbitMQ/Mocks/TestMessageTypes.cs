using NetFusion.Messaging.Types;

namespace NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

public class TestDomainEvent(int valueOne, int valueTwo) : DomainEvent
{
    public int ValueOne { get; } = valueOne;
    public int ValueTwo { get; } = valueTwo;
}

public class TestDomainEventHandler
{
    public void OnDomainEvent(TestDomainEvent domainEvent)
    {

    }
}

public class TestCommand : Command;

public class TestCommandHandler
{
    public void OnCommand(TestCommand command)
    {
        
    }
}

public class TestCommandResponse : Command;

public class TestCommandWithResponse : Command<TestCommandResponse>;

public class TestCommandHandlerWithResponse
{
    public TestCommandResponse OnCommand(TestCommandWithResponse command)
    {
        return new TestCommandResponse();
    }
}

public class TestReplyQueueHandler
{
    public void OnReply(TestCommandResponse response)
    {
        
    }
}