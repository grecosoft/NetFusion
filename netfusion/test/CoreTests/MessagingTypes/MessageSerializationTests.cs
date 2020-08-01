using System.Text.Json;
using FluentAssertions;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Serialization;
using Xunit;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace CoreTests.MessagingTypes
{
    /// <summary>
    /// These tests assure the correct serialization of message derived classes.
    /// The base message type has a property named Attributes that can contain
    /// a set of key/value pairs.  Both the key and value are string types so
    /// the data can be easily sent out of process and deserialized.  
    /// </summary>
    public class MessageSerializationTests
    {
        [Fact]
        public void DomainEvent_UsingMsJson_SerializesCorrectly()
        {
            var domainEvent = new TestDomainEvent(500)
            {
                TestProp1 = "Test Value1"
            };

            domainEvent.Attributes.SetIntValue("TestAttrib1", 60);
            
            var json = JsonSerializer.Serialize(domainEvent);
            var newDomainEvent = JsonSerializer.Deserialize<TestDomainEvent>(json);

            newDomainEvent.TestProp1.Should().Be(domainEvent.TestProp1);
            newDomainEvent.Attributes.GetIntValue("TestAttrib1").Should().Be(60);
        }

       [Fact]
        public void Command_UsingMsJson_SerializesCorrectly()
        {
            var command = new TestCommand(500)
            {
                TestProp1 = "Test Value1"
            };

            command.Attributes.SetIntValue("TestAttrib1", 60);
            
            var json = JsonSerializer.Serialize(command);
            var newCommand = JsonSerializer.Deserialize<TestCommand>(json);

            newCommand.TestProp1.Should().Be(command.TestProp1);
            newCommand.Attributes.GetIntValue("TestAttrib1").Should().Be(60);
        }

        [Fact]
        public void DomainEvent_UsingMessagePack_SerializesCorrectly()
        {
            var serializer = new MessagePackSerializer();
            var domainEvent = new TestDomainEvent(500)
            {
                TestProp1 = "Test Value1"
            };

            domainEvent.Attributes.SetIntValue("TestAttrib1", 60);
            
            byte[] data = serializer.Serialize(domainEvent);
            var newDomainEvent = serializer.Deserialize<TestDomainEvent>(data, typeof(TestDomainEvent));

            newDomainEvent.TestProp1.Should().Be(domainEvent.TestProp1);
            newDomainEvent.TestProp2.Should().Be(newDomainEvent.TestProp2);
            newDomainEvent.Attributes.GetIntValue("TestAttrib1").Should().Be(60);
        }

        [Fact]
        public void Command_UsingMessagePack_SerializesCorrectly()
        {
            var serializer = new MessagePackSerializer();
            var command = new TestCommand(500)
            {
                TestProp1 = "Test Value1"
            };

            command.Attributes.SetIntValue("TestAttrib1", 60);
            
            var data = serializer.Serialize(command);
            var newCommand = serializer.Deserialize<TestCommand>(data, typeof(TestCommand));

            newCommand.TestProp1.Should().Be(command.TestProp1);
            command.TestProp2.Should().Be(newCommand.TestProp2);
            newCommand.Attributes.GetIntValue("TestAttrib1").Should().Be(60);
        }

        public class TestDomainEvent : DomainEvent
        {
            public string TestProp1 { get; set; }
            public int TestProp2 { get; private set; }

            public TestDomainEvent() { }

            public TestDomainEvent(int testProp2)
            {
                TestProp2 = testProp2;
            }
        }

        public class TestCommand : Command<TestResult>
        {
            public string TestProp1 { get; set; }
            public int TestProp2 { get; private set; }

            public TestCommand() { }

            public TestCommand(int testProp2)
            {
                TestProp2 = testProp2;
            }
        }

        public class TestResult
        {
        }
    }
}