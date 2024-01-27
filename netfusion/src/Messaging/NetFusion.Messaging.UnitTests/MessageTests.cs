using System.Text.Json;
using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Contracts;

// ReSharper disable ClassNeverInstantiated.Local
namespace NetFusion.Messaging.UnitTests;

public class MessageTests
{
    /// <summary>
    /// The model returned from a query can derive from the base QueryReadModel.
    /// This allows additional non-type properties to be added to the result.
    /// </summary>
    [Fact]
    public void QueryCanReturn_DynamicModel()
    {
        // Publisher...
        var query = new QueryDevice(100);
            
        // Subscriber ...
        var deviceModel = new DeviceModel
        {
            DeviceId = 100,
            Name = "Temperature",
            Location = "Office"
        };

        var expectedDate = DateTime.UtcNow;
        deviceModel.Attributes.Values.LastRead = expectedDate;
        deviceModel.Attributes.Values.Version = "1.4.6";
        deviceModel.AttributeValues["Sequence"] = 29;
            
        // Assert:
        query.SetResult(deviceModel);
        query.Result.Should().BeSameAs(deviceModel);
        Assert.True(query.Result.Attributes.Values.LastRead == expectedDate);
        Assert.True(query.Result.Attributes.Values.Version == "1.4.6");
        Assert.True(query.Result.Attributes.Values.Sequence == 29);

        query.Result.AttributeValues.Should().ContainKey("LastRead");
        query.Result.AttributeValues.Should().ContainKey("Version");
                
        // Can be serialized to JSON.  While deserialization is possible, since the base
        // dynamic type is based on a string/object dictionary the values are deserialized
        // to a JsonElement and not the original type (so consumer has some additional work).
        var json = JsonSerializer.Serialize(query.Result);
        json.Should().NotBeNull();

        var result = JsonSerializer.Deserialize<DeviceModel>(json);
        result.Should().NotBeNull();
        result!.AttributeValues["Sequence"].Should().BeOfType<JsonElement>();
    }

    /// <summary>
    /// Query read model can also have its dynamic state initialized
    /// from a dictionary.
    /// </summary>
    [Fact]
    public void QueryReadModel_CanBePopulated()
    {
        var state = new Dictionary<string, object?>
        {
            { "Port", 4000 },
            { "State", "Active" }
        };

        var device = new DeviceModel
        {
            AttributeValues = state
        };

        Assert.True(device.Attributes.Values.Port == 4000);
        Assert.True(device.Attributes.Values.State == "Active");
    }

    /// <summary>
    /// A message can have an associated namespace.  This is used to provide scope
    /// to message used by consumers. 
    /// </summary>
    [Fact]
    public void Message_CanHave_Namespace()
    {
        var nsAttrib = new MockMessage().GetAttribute<MessageNamespaceAttribute>();
        nsAttrib.Should().NotBeNull();
        nsAttrib!.MessageNamespace.Should().Be("Biz.Domain.Messages.Mock");
    }

    [Fact]
    public void CommandResult_MustBeAssignable_ToCommandResultType()
    {
        IMessageWithResult cmdState = new MockCommand();
        cmdState.SetResult(new CommandResultOne());
        cmdState.MessageResult.Should().NotBeNull();

        Assert.Throws<InvalidOperationException>(() =>
        {
            cmdState.SetResult(new CommandResultTwo());
        }).Message.Should().Contain("and is not assignable to the command's declared result type");
    }

    //--------- TEST CLASSES ----------------

    [MessageNamespace("Biz.Domain.Messages.Mock")]
    private class MockMessage : DomainEvent
    {
        public decimal Amount { get; set; }
    }
        
    public class QueryDevice : Query<DeviceModel>
    {
        public int DeviceId { get; }
            
        public QueryDevice(int deviceId)
        {
            DeviceId = deviceId;
        }
    }

    public class DeviceModel : QueryReadModel
    {
        public int DeviceId { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
    }

    public class MockCommand : Command<CommandResultBase>;

    public abstract class CommandResultBase;

    public class CommandResultOne : CommandResultBase;

    public class CommandResultTwo;
}