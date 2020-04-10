using System;
using NetFusion.Messaging.Types;
using Xunit;
using FluentAssertions;
using NetFusion.Messaging.Types.Attributes;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Provides extension method tests for reading basic types and array of basic types stored within a dictionary
    /// where the key is the name and the value is the string representation.  This is being used so deserialization
    /// will not be dependent on the specific serializer being used.  (JSON serializers and MessagePack serializers
    /// will not know the exact type if object was used for the dictionary's type.  These serializers will deserialize
    /// the value into an object representing the value).  
    /// </summary>
    public class MessageAttributeTests
    {
        [Fact]
        public void MessagesHave_ArbitraryAttributes()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.Should().NotBeNull();
            
            var command = new TestCommand();
            command.Attributes.Should().NotBeNull();
        }

        [Fact]
        public void MessageAttributeNames_CaseInsensitive()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetIntValue("Int-Value", 1000);

            int value = domainEvent.Attributes.GetIntValue("int-value", 2000);
            value.Should().Be(1000);
        }

        [Fact]
        public void InvalidAttributeName_ExceptionRaised()
        {
            var domainEvent = new TestDomainEvent();

            var exception = Assert.Throws<InvalidOperationException>(
                () => domainEvent.Attributes.GetIntValue("Missing"));

            exception.Message.Should().Contain("Attribute named: Missing not found.");

        }

        [Fact]
        public void AttributeStringValue_Supported()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetStringValue("TestValue", "test value");
            domainEvent.Attributes.GetStringValue("TestValue").Should().Be("test value");
            domainEvent.Attributes.GetStringValue("TestValue2", "default value").Should().Be("default value");
        }
        
        [Fact]
        public void AttributeIntValue_Supported()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetIntValue("TestValue", 100);
            domainEvent.Attributes.SetIntValue("TestArray", new[] { 200, 400 });

            domainEvent.Attributes.GetIntValue("TestValue").Should().Be(100);
            domainEvent.Attributes.GetIntArrayValue("TestArray").Should().BeEquivalentTo(new[] { 200, 400 });
            domainEvent.Attributes.GetIntValue("TestValue2", 699).Should().Be(699);
        }
        
        [Fact]
        public void AttributeByteValue_Supported()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetByteValue("TestValue", 5);
            domainEvent.Attributes.GetByteValue("TestValue").Should().Be(5);
        }
        
        [Fact]
        public void AttributeDecimalValue_Supported()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetDecimalValue("TestValue", 100.98M);
            domainEvent.Attributes.SetDecimalValue("TestArray", new[] { 20.88M, 55.9802M });

            domainEvent.Attributes.GetDecimalValue("TestValue").Should().Be(100.98M);
            domainEvent.Attributes.GetDecimalArrayValue("TestArray").Should().BeEquivalentTo(new[] { 20.88M, 55.9802M });
            domainEvent.Attributes.GetDecimalValue("TestValue2", 887.88M).Should().Be(887.88M);
        }
        
        [Fact]
        public void AttributeBoolValue_Supported()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetBoolValue("TestValue", true);
            domainEvent.Attributes.GetBoolValue("TestValue").Should().BeTrue();
        }
        
        [Fact]
        public void AttributeGuidValue_Supported()
        {
            var guid = new Guid();
            var defaultGuid = new Guid();
            
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetGuidValue("TestValue", guid);
            domainEvent.Attributes.GetGuidValue("TestValue").Should().Be(guid);
            domainEvent.Attributes.GetGuidValue("TestValue2", defaultGuid).Should().Be(defaultGuid);
        }
        
        [Fact]
        public void AttributeDateTimeValue_Supported()
        {
            var utcDate = DateTime.UtcNow;
            var utcDefault = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2));
            var localDate = DateTime.Now;
            var localDefault = DateTime.Now.Subtract(TimeSpan.FromDays(5));
            
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetDateValue("TestValue", utcDate);
            domainEvent.Attributes.GetUtcDateTimeValue("TestValue").Should().Be(utcDate);
            domainEvent.Attributes.GetUtcDateTimeValue("TestValue2", utcDefault).Should().Be(utcDefault);
            domainEvent.Attributes.GetUtcDateTimeValue("TestValue3", localDate).Should().Be(localDate.ToUniversalTime());

            domainEvent.Attributes.SetDateValue("TestValue10", localDate);
            domainEvent.Attributes.GetDateTimeValue("TestValue10").Should().Be(localDate);
            domainEvent.Attributes.GetDateTimeValue("TestValue11", localDefault).Should().Be(localDefault);
        }
        
        [Fact]
        public void AttributeUIntValue_Supported()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetUIntValue("TestValue", 966);
            domainEvent.Attributes.GetUIntValue("TestValue").Should().Be(966);
        }

        [Fact]
        public void ExistingAttributeOverriden_ByDefault()
        {
            
        }

        [Fact]
        public void ExistingAttribute_CanBePreserved()
        {
            
        }

        private class TestDomainEvent : DomainEvent
        {
            
        }

        private class TestCommand : Command
        {
            
        }
    }
}