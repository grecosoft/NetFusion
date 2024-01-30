using NetFusion.Messaging.Types.Attributes;
using NetFusion.Services.Serialization;

namespace NetFusion.Services.UnitTests.Serialization;

public class MessagePackSerializationMessageTests
{
    [Fact]
    public void Serialize_Command_WithNoSetResult()
    {
        // Arrange:
        var device = new RegisterDevice(Guid.NewGuid().ToString(), "2.0.0.5", "Test Device");
        
        // Act:
        var serializer = new MessagePackSerializer();
        var data = serializer.Serialize(device);
        var command = (RegisterDevice?)serializer.Deserialize(data, typeof(RegisterDevice));

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
        var serializer = new MessagePackSerializer();
        var data = serializer.Serialize(device);
        var command = (RegisterDevice?)serializer.Deserialize(data, typeof(RegisterDevice));

        // Assert:
        command.Should().NotBeNull();
        command.Should().BeEquivalentTo(device);
    }
    
    [Fact]
    public void Serialize_Command_WithAttributes()
    {
        // Arrange:
        var device = new RegisterDevice(Guid.NewGuid().ToString(), "2.0.0.5", "Test Device");
        device.Attributes.SetIntValue("TestAttribute", new[] { 10, 20, 30 });
        
        // Act:
        var serializer = new MessagePackSerializer();
        var data = serializer.Serialize(device);
        var command = (RegisterDevice?)serializer.Deserialize(data, typeof(RegisterDevice));

        // Assert:
        command.Should().NotBeNull();
        command.Should().BeEquivalentTo(device);
        command!.Attributes.GetIntArrayValue("TestAttribute").Should().BeEquivalentTo(new [] { 10, 20, 30 });
    }
    
    [Fact]
    public void Serialize_Command_WithNullableResult()
    {
        // Arrange:
        var device = new RegisterDeviceNullableResult(Guid.NewGuid().ToString(), "2.0.0.5", "Test Device");
        device.SetResult(null);
        
        // Act:
        var serializer = new MessagePackSerializer();
        var data = serializer.Serialize(device);
        var command = (RegisterDevice?)serializer.Deserialize(data, typeof(RegisterDevice));

        // Assert:
        command.Should().NotBeNull();
        command.Should().BeEquivalentTo(device);
        command!.Result.Should().BeNull();
    }
    
    // Test message types compatible with MessagePack Serialization:
    
    [Fact]
    public void Serialize_Event()
    {
        // Arrange:
        var message = new DeviceUpdated(Guid.NewGuid().ToString(), "2.0.0.5", "2.0.0.10");
        
        // Act:
        var serializer = new MessagePackSerializer();
        var data = serializer.Serialize(message);
        var domainEvt = (DeviceUpdated?)serializer.Deserialize(data, typeof(DeviceUpdated));

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
        var serializer = new MessagePackSerializer();
        var data = serializer.Serialize(message);
        var domainEvt = (DeviceUpdated?)serializer.Deserialize(data, typeof(DeviceUpdated));

        // Assert:
        domainEvt.Should().NotBeNull();
        domainEvt.Should().BeEquivalentTo(message);
        domainEvt!.Attributes.GetTimeSpanValue("PingDuration").Should().Be(TimeSpan.FromSeconds(30));
    }
    
    public class RegistrationStatus(string registrationId, string status, string address)
    {
        public string RegistrationId { get; } = registrationId;
        public string Status { get; } = status;
        public string Address { get; } = address;
    }
    
    public class RegisterDevice(string deviceId, string version, string name) : Command<RegistrationStatus>
    {
        public string DeviceId { get; private set; } = deviceId;
        public string Version { get; private set; } = version;
        public string Name { get; private set; } = name;

        public RegisterDevice(string deviceId, string version, string name, 
            IDictionary<string, string> attributes,
            RegistrationStatus? result) : this (deviceId, version, name)
        {
            Attributes = attributes;
            SetResult(result);
        }
    }
    
    public class RegisterDeviceNullableResult(string deviceId, string version, string name)
        : Command<RegistrationStatus?>
    {
        public string DeviceId { get; } = deviceId;
        public string Version { get; } = version;
        public string Name { get; } = name;

        public RegisterDeviceNullableResult(string deviceId, string version, string name, 
            IDictionary<string, string> attributes,
            RegistrationStatus? result) : this (deviceId, version, name)
        {
            Attributes = attributes;
            SetResult(result);
        }
    }
    
    public class DeviceUpdated(string deviceId, string priorVersion, string currentVersion)
        : DomainEvent
    {
        public string DeviceId { get; } = deviceId;
        public string PriorVersion { get; } = priorVersion;
        public string CurrentVersion { get; } = currentVersion;

        public DeviceUpdated(string deviceId, string priorVersion, string currentVersion, 
            IDictionary<string, string> attributes) : this(deviceId, priorVersion, currentVersion)
        {
            Attributes = attributes;
        }
    }
}