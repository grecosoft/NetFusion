using CoreTests.Domain.Mocks;
using FluentAssertions;
using NetFusion.Domain.Entities;
using System.Linq;
using Xunit;

namespace CoreTests.Domain
{
    public class AttributedEntityTests
    {
        [Fact(DisplayName = nameof(SetAndRetrieve_EntityAttribute))]
        public void SetAndRetrieve_EntityAttribute()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            var value = entity.Attributes.Values.Value1 as object;
            value.Should().NotBeNull();
            value.Should().BeOfType<int>();

            var typedValue = (int)value;
            typedValue.Should().Be(1000);
        }

        [Fact(DisplayName = nameof(Delete_EntityAttribute))]
        public void Delete_EntityAttribute()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            var isDeleted = entity.Attributes.Delete("Value1");
            isDeleted.Should().BeTrue();

            isDeleted = entity.Attributes.Delete("Value1");
            isDeleted.Should().BeFalse();
        }

        [Fact(DisplayName = nameof(Check_EntityAttributeExits))]
        public void Check_EntityAttributeExits()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            entity.Attributes.Contains("Value1").Should().BeTrue();
            entity.Attributes.Contains("Value2").Should().BeFalse();
        }

        [Fact(DisplayName = nameof(GetTyped_EntityAttributeValue))]
        public void GetTyped_EntityAttributeValue()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("Value1", 1000);

            var value = entity.Attributes.GetValue<int>("Value1");
            value.Should().Be(1000);
        }

        [Fact(DisplayName = nameof(RequestDefaultValue_WhenAttributeNotPresent))]
        public void RequestDefaultValue_WhenAttributeNotPresent()
        {
            var entity = new DynamicEntity();
            var value = entity.Attributes.GetValueOrDefault<int>("Value1");
            value.Should().Be(0);
        }

        [Fact(DisplayName = nameof(RequestSpecificDefaultValue_WhenAttributeNotPresent))]
        public void RequestSpecificDefaultValue_WhenAttributeNotPresent()
        {
            var entity = new DynamicEntity();
            var value = entity.Attributes.GetValueOrDefault<int>("Value1",  1000);
            value.Should().Be(1000);
        }

        [Fact(DisplayName = nameof(EntityAttributePresent_DefaultValueNotReturned))]
        public void EntityAttributePresent_DefaultValueNotReturned()
        {
            var entity = new DynamicEntity();
            entity.Attributes.Values.Value1 = 1000;
            var value = entity.Attributes.GetValueOrDefault<int>("Value1", 2000);
            value.Should().Be(1000);
        }

        [Fact(DisplayName = nameof(AssociateContext_WithAttribute))]
        public void AssociateContext_WithAttribute()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("TestValue", 7777, typeof(SampleContext));
            entity.AttributeValues.Keys.Should().HaveCount(1);

            var key = entity.AttributeValues.Keys.First();
            key.Should().Be(typeof(SampleContext).Namespace + "-TestValue");
        }

        [Fact(DisplayName = nameof(ReceiveValue_AssociatedWithContext))]
        public void ReceiveValue_AssociatedWithContext()
        {
            var entity = new DynamicEntity();
            entity.Attributes.SetValue("TestValue", 7777, typeof(SampleContext));

            var value = entity.Attributes.GetValue<int>("TestValue", typeof(SampleContext));
        }

        [Fact(DisplayName = nameof(CallingMethodNameUsed_WhenNameNotSpecified))]
        public void CallingMethodNameUsed_WhenNameNotSpecified()
        {
            var entity = new DynamicEntity();
            entity.SetValueWithNoName(1000);

            var value = entity.GetValueWithNoName();
            value.Should().Be(1000);

            entity.AttributeValues.Should().HaveCount(1);
            entity.AttributeValues.First().Key.Should().Be("ValueWithNoName");
        }

        [Fact(DisplayName = nameof(ContextAssociatedValueNotPresent_DefaultValueUsed))]
        public void ContextAssociatedValueNotPresent_DefaultValueUsed()
        {
            var entity = new DynamicEntity();

            var value = entity.Attributes.GetValueOrDefault<int>("TestValue", 8000, typeof(SampleContext));
            value.Should().Be(8000);
        }
    }

    public static class SampleContext
    {
        public static int GetValueWithNoName(this IAttributedEntity entity)
        {
            return entity.Attributes.GetMemberValueOrDefault<int>();
        }

        public static void SetValueWithNoName(this IAttributedEntity entity, int value)
        {
            entity.Attributes.SetMemberValue(value);
        }
    }
}

