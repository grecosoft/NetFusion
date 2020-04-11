using System.Text.Json;
using FluentAssertions;
using NetFusion.Messaging.Types;
using Xunit;
using NetFusion.Messaging.Types.Attributes;

namespace CoreTests.Messaging
{
    /// <summary>
    /// These tests assure the correct serialization of message
    /// derived classes.
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

            // In the future release, properties with private setters will
            // be supported by extending MS serializer.
            // newDomainEvent.TestProp2.Should().Be(newDomainEvent.TestProp2);

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

            // In the future release, properties with private setters will
            // be supported by extending MS serializer.
            // command.TestProp2.Should().Be(newCommand.TestProp2);

            newCommand.Attributes.GetIntValue("TestAttrib1").Should().Be(60);
        }

        [Fact]
        public void DomainEvent_UsingMessagePack_SerializesCorrectly()
        {

        }

        [Fact]
        public void Command_UsingMessagePack_SerializesCorrectly()
        {

        }

        private class TestDomainEvent : DomainEvent
        {
            public string TestProp1 { get; set; }
            public int TestProp2 { get; private set; }

            public TestDomainEvent() { }

            public TestDomainEvent(int testProp2)
            {
                TestProp2 = testProp2;
            }
        }

        private class TestCommand : Command<TestResult>
        {
            public string TestProp1 { get; set; }
            public int TestProp2 { get; private set; }

            public TestCommand() { }

            public TestCommand(int testProp2)
            {
                TestProp2 = testProp2;
            }
        }

        private class TestResult
        {
        }
        
    }
}