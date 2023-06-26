using FluentAssertions;
using NetFusion.Integration.Bus;
using NetFusion.Integration.UnitTests.Bus.Mocks;
using NetFusion.Messaging.Types.Attributes;

namespace NetFusion.Integration.UnitTests.Bus;

/// <summary>
/// The ReplyTo queue can be specified as a message attribute - usually on a command
/// indicating to which queue the response should be sent.  The value consists of the
/// bus and queue names separated by a colon. The bus name corresponds to the name of
/// the configuration for the bus and is agreed by the publishing and consuming
/// microservices. 
/// </summary>
public class MessageUnitTests
{

    [Fact]
    public void ReplyQueueAndBus_CanBeDetermined_FromMessageAttribute()
    {
        var command = new TestCommand();
        command.SetReplyTo("testBus:ReplyQueueName");

        command.TryParseReplyTo(out string? busName, out string? queueName).Should().BeTrue();
        busName.Should().Be("testBus");
        queueName.Should().Be("ReplyQueueName");
        
    }

    [Fact]
    public void ReplyQueueAndBus_CanBeDetermined_FromEncodedStringValue()
    {
        MessageExtensions.TryParseReplyTo("testBus:ReplyQueueName",
            out string? busName, 
            out string? queueName).Should().BeTrue();
        
        busName.Should().Be("testBus");
        queueName.Should().Be("ReplyQueueName");
    }
}