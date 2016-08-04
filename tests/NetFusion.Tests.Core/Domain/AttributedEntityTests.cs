using NetFusion.Tests.Core.Domain.Mocks;
using Xunit;
using FluentAssertions;
using System.Linq;
using NetFusion.Domain.Entity;

namespace NetFusion.Tests.Core.Domain
{
    public class AttributedEntityTests
    {
        [Fact]
        public void CanSetAndRetrieveEntityAttribute()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            var value = entity.Attributes.Values.Value1 as object;
            value.Should().NotBeNull();
            value.Should().BeOfType<int>();

            var typedValue = (int)value;
            typedValue.Should().Be(1000);
        }

        [Fact]
        public void CanDeleteEntityAttribute()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            var isDeleted = entity.Attributes.Delete("Value1");
            isDeleted.Should().BeTrue();

            isDeleted = entity.Attributes.Delete("Value1");
            isDeleted.Should().BeFalse();
        }

        [Fact]
        public void CanCheckIfEntityAttributeExits()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            entity.Attributes.Contains("Value1").Should().BeTrue();
            entity.Attributes.Contains("Value2").Should().BeFalse();
        }

        [Fact]
        public void CanGetTypedEntityAttributeValue()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            var value = entity.Attributes.GetValue<int>("Value1");
            value.Should().Be(1000);
        }

        [Fact]
        public void CanRequestDefaultValueIfEntityAttributeNotPresent()
        {
            var entity = new DynamicEntity();
            var value = entity.Attributes.GetValueOrDefault<int>("Value1");
            value.Should().Be(0);
        }

        [Fact]
        public void CanRequestSpecificDefaultValueIfEntityAttributeNotPresent()
        {
            var entity = new DynamicEntity();
            var value = entity.Attributes.GetValueOrDefault<int>("Value1", 1000);
            value.Should().Be(1000);
        }

        [Fact]
        public void IfEntityAttributePresentDefaultValueNotReturned()
        {
            var entity = new DynamicEntity();
            entity.Attributes.Values.Value1 = 1000;
            var value = entity.Attributes.GetValueOrDefault<int>("Value1", 2000);
            value.Should().Be(1000);
        }

        [Fact]
        public void CanAssociateContextWithValue()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue(7777, typeof(SampleContext), "TestValue");
            entity.AttributeValues.Keys.Should().HaveCount(1);

            var key = entity.AttributeValues.Keys.First();
            key.Should().Be(typeof(SampleContext).FullName + "-TestValue");
        }

        [Fact]
        public void CanReceiveValueAssociatedWithContext()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue(7777, typeof(SampleContext), "TestValue");

            var value = entity.Attributes.GetValue<int>(typeof(SampleContext), "TestValue");
        }
        
        [Fact]
        public void CallingMethodNameWillBeUsedIfNameNotSpecified()
        {
            var entity = new DynamicEntity();
            entity.SetValueWithNoName(1000);

            var value = entity.GetValueWithNoName();
            value.Should().Be(1000);

            entity.AttributeValues.Should().HaveCount(1);
            entity.AttributeValues.First().Key.Should().Be("ValueWithNoName");
        }

        [Fact]
        public void IfContextAssociatedValueNotPresentDefaultValueCanBeUsed()
        {
            var entity = new DynamicEntity();

            var value = entity.Attributes.GetValue<int>(8000, typeof(SampleContext), "TestValue");
            value.Should().Be(8000);
        }
    }

    public static class SampleContext
    {
        public static int GetValueWithNoName(this IAttributedEntity entity)
        {
            return entity.Attributes.GetValue<int>();
        }

        public static void SetValueWithNoName(this IAttributedEntity entity, int value)
        {
            entity.Attributes.SetValue(value);
        }
    }
}

