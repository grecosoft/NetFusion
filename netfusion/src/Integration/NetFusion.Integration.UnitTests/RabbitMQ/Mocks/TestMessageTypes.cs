using NetFusion.Messaging.Types;

namespace NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

public class TestDomainEvent : DomainEvent
{
    public int ValueOne { get; }
    public int ValueTwo { get; }

    public TestDomainEvent(int valueOne, int valueTwo)
    {
        ValueOne = valueOne;
        ValueTwo = valueTwo;
    }
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