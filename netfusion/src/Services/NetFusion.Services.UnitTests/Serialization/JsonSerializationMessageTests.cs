using NetFusion.Messaging.Types.Attributes;
using NetFusion.Services.Serialization;

namespace NetFusion.Services.UnitTests.Serialization;

public class JsonSerializationMessageTests
{
    [Fact]
    public void Serialize_Command_WithNoSetResult()
    {
        // Arrange:
        var device = new RegisterDevice(Guid.NewGuid().ToString(), "2.0.0.5", "Test Device");
        
        // Act:
        var serializer = new JsonMessageSerializer();
        var data = serializer.Serialize(device);
        var command = serializer.Deserialize(data, typeof(RegisterDevice));

        // Assert:
        command.Should().BeEquivalentTo(device);
    }

    [Fact]
    public void Serialize_Command_WithSetResult()
    {
        // Arrange:
        var device = new RegisterDevice(Guid.NewGuid().ToString(), "2.0.0.5", "Test Device");
        var result = new RegistrationStatus(Guid.NewGuid().ToString(), "OFF_LINE", "344.999.4322");
        device.SetResult(result);
        
        // Act:
        var serializer = new JsonMessageSerializer();
        var data = serializer.Serialize(device);
        var command = (RegisterDevice?)serializer.Deserialize(data, typeof(RegisterDevice));

        // Assert:
        command.Should().NotBeNull();
        command.Should().BeEquivalentTo(device);
    }

    [Fact]
    public void Serialize_Command_WithNullableResult()
    {
        // Arrange:
        var device = new RegisterDeviceNullableResult(Guid.NewGuid().ToString(), "2.0.0.5", "Test Device");
        device.SetResult(null);
        
        // Act:
        var serializer = new JsonMessageSerializer();
        var data = serializer.Serialize(device);
        var command = (RegisterDevice?)serializer.Deserialize(data, typeof(RegisterDevice));

        // Assert:
        command.Should().NotBeNull();
        command.Should().BeEquivalentTo(device);
        command!.Result.Should().BeNull();
    }
    
    [Fact]
    public void Serialize_Command_WithAttributes()
    {
        // Arrange:
        var device = new RegisterDevice(Guid.NewGuid().ToString(), "2.0.0.5", "Test Device");
        device.Attributes.SetIntValue("TestAttribute", new[] { 10, 20, 30 });
        
        // Act:
        var serializer = new JsonMessageSerializer();
        var data = serializer.Serialize(device);
        var command = (RegisterDevice?)serializer.Deserialize(data, typeof(RegisterDevice));

        // Assert:
        command.Should().NotBeNull();
        command.Should().BeEquivalentTo(device);
        command!.Attributes.GetIntArrayValue("TestAttribute").Should().BeEquivalentTo(new [] { 10, 20, 30 });
    }
    
    [Fact]
    public void Serialize_Event()
    {
        // Arrange:
        var message = new DeviceUpdated(Guid.NewGuid().ToString(), "2.0.0.5", "2.0.0.10");
        
        // Act:
        var serializer = new JsonMessageSerializer();
        var json = serializer.Serialize(message);
        var domainEvt = (DeviceUpdated?)serializer.Deserialize(json, typeof(DeviceUpdated));

        // Assert:
        domainEvt.Should().BeEquivalentTo(message);
    }

    [Fact]
    public void Serialize_Event_WithAttributes()
    {
        // Arrange:
        var message = new DeviceUpdated(Guid.NewGuid().ToString(), "2.0.0.5", "2.0.0.10");
        message.Attributes.SetTimeSpan("PingDuration", TimeSpan.FromSeconds(30));
        
        // Act:
        var serializer = new JsonMessageSerializer();
        var json = serializer.Serialize(message);
        var domainEvt = (DeviceUpdated?)serializer.Deserialize(json, typeof(DeviceUpdated));

        // Assert:
        domainEvt.Should().NotBeNull();
        domainEvt.Should().BeEquivalentTo(message);
        domainEvt!.Attributes.GetTimeSpanValue("PingDuration").Should().Be(TimeSpan.FromSeconds(30));
    }
    
    // Test message types compatible with JSON Serialization:
    
    public class RegistrationStatus
    {
        public string RegistrationId { get; }
        public string Status { get; }
        public string Address { get; }
    
        public RegistrationStatus(string registrationId, string status, string address)
        {
            RegistrationId = registrationId;
            Status = status;
            Address = address;
        }
    }
    
    public class RegisterDevice(string deviceId, string version, string name) : Command<RegistrationStatus>
    {
        public string DeviceId { get; private set; } = deviceId;
        public string Version { get; private set; } = version;
        public string Name { get; private set; } = name;
    }
    
    public class RegisterDeviceNullableResult(string deviceId, string version, string name)
        : Command<RegistrationStatus?>
    {
        public string DeviceId { get; } = deviceId;
        public string Version { get; } = version;
        public string Name { get; } = name;
    }
    
    public class DeviceUpdated(string deviceId, string priorVersion, string currentVersion)
        : DomainEvent
    {
        public string DeviceId { get; } = deviceId;
        public string PriorVersion { get; } = priorVersion;
        public string CurrentVersion { get; } = currentVersion;
    }
}

