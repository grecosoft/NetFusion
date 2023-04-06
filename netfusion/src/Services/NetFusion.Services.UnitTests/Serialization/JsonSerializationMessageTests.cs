using FluentAssertions;
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
    
    public class RegisterDevice : Command<RegistrationStatus>
    {
        public string DeviceId { get; private set; }
        public string Version { get; private set; }
        public string Name { get; private set; }
        
        public RegisterDevice(string deviceId, string version, string name)
        {
            DeviceId = deviceId;
            Version = version;
            Name = name;
        }
    }
    
    public class RegisterDeviceNullableResult : Command<RegistrationStatus?>
    {
        public string DeviceId { get; }
        public string Version { get; }
        public string Name { get; }
    
        public RegisterDeviceNullableResult(string deviceId, string version, string name)
        {
            DeviceId = deviceId;
            Version = version;
            Name = name;
        }
    }
    
    public class DeviceUpdated : DomainEvent
    {
        public string DeviceId { get; }
        public string PriorVersion { get; }
        public string CurrentVersion { get; }
        
        public DeviceUpdated(string deviceId, string priorVersion, string currentVersion)
        {
            DeviceId = deviceId;
            PriorVersion = priorVersion;
            CurrentVersion = currentVersion;
        }
    }
}

