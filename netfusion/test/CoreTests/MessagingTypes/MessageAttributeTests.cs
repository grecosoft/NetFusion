using System;
using FluentAssertions;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Attributes;
using Xunit;

namespace CoreTests.MessagingTypes
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

            Assert.Throws<InvalidOperationException>(
                () => domainEvent.Attributes.GetIntValue("Missing")
            ).Message.Should().Contain("Attribute named: Missing not found.");

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
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetIntValue("TestValue", 5);
            domainEvent.Attributes.SetIntValue("TestValue", 2);

            domainEvent.Attributes.GetIntValue("TestValue").Should().Be(2);
        }

        [Fact]
        public void ExistingAttribute_CanBePreserved()
        {
            var domainEvent = new TestDomainEvent();
            domainEvent.Attributes.SetIntValue("TestValue", 5);
            domainEvent.Attributes.SetIntValue("TestValue", 10, false);

            domainEvent.Attributes.GetIntValue("TestValue").Should().Be(5);
        }

        [Fact]
        public void CanSetCorrelationId()
        {
            var expectedValue = Guid.NewGuid().ToString();
            
            var domainEvent = new TestDomainEvent();
            domainEvent.SetCorrelationId(expectedValue);
            domainEvent.GetCorrelationId().Should().Be(expectedValue);
        }
        
        [Fact]
        public void CanSetUtcDateOccurred()
        {
            var expectedValue = DateTime.Now;
            var expectedUtcValue = expectedValue.ToUniversalTime();
            
            var domainEvent = new TestDomainEvent();
            domainEvent.SetUtcDateOccurred(expectedValue);
            domainEvent.GetUtcDateOccurred().Should().Be(expectedUtcValue);
        }
        
        [Fact]
        public void CanSetRouteKey()
        {
            var domainEvent = new TestDomainEvent();
            const string expectedValue = "Hartford";
            
            domainEvent.SetRouteKey(expectedValue);
            domainEvent.GetRouteKey().Should().Be(expectedValue);
            
            domainEvent.SetRouteKey("CT", "Hartford", "West");
            domainEvent.GetRouteKey().Should().Be("CT.Hartford.West");
        }
        
        [Fact]
        public void CanSetPriority()
        {
            var domainEvent = new TestDomainEvent();
            const byte expectedValue = 10;

            domainEvent.GetPriority().Should().BeNull();
            domainEvent.SetPriority(expectedValue);
            domainEvent.GetPriority().Should().Be(expectedValue);
        }

        [Fact]
        public void CanSetMessageId()
        {
            var domainEvent = new TestDomainEvent();
            const string expectedValue = "NC_839472";
            
            domainEvent.SetMessageId(expectedValue);
            domainEvent.GetMessageId().Should().Be(expectedValue);
        }

        [Fact]
        public void CanSetSubject()
        {
            var domainEvent = new TestDomainEvent();
            const string expectedValue = "LatestPurchaseOrder";
            
            domainEvent.SetSubject(expectedValue);
            domainEvent.GetSubject().Should().Be(expectedValue);
        }

        [Fact]
        public void CanSetReplyTo()
        {
            var domainEvent = new TestDomainEvent();
            const string expectedValue = "OrderStatusQueue";
            
            domainEvent.SetReplyTo(expectedValue);
            domainEvent.GetReplyTo().Should().Be(expectedValue);
        }
        
        [Fact]
        public void CanSetTo()
        {
            var domainEvent = new TestDomainEvent();
            const string expectedValue = "InventoryLow";
            
            domainEvent.SetTo(expectedValue);
            domainEvent.GetTo().Should().Be(expectedValue);
        }

        [Fact]
        public void CanSetUserId()
        {
            var domainEvent = new TestDomainEvent();
            const string expectedValue = "mark.twain@house.com";

            domainEvent.SetUserId(expectedValue);
            domainEvent.GetUserId().Should().Be(expectedValue);
        }

        [Fact]
        public void CanSetAbsoluteExpiryTime()
        {
            var domainEvent = new TestDomainEvent();
            var expectedValue = DateTime.Now.AddDays(5);
            var expectedUtcValue = expectedValue.ToUniversalTime();

            domainEvent.SetUtcAbsoluteExpiryTime(expectedValue);
            domainEvent.GetUtcAbsoluteExpiryTime().Should().Be(expectedUtcValue);
        }

        [Fact]
        public void CanSetTls()
        {
            var domainEvent = new TestDomainEvent();
            const uint expectedValue = uint.MaxValue;
            
            domainEvent.SetTls(expectedValue);
            domainEvent.GetTls().Should().Be(expectedValue);
        }

        private class TestDomainEvent : DomainEvent
        {
            
        }

        private class TestCommand : Command
        {
            
        }
    }
}