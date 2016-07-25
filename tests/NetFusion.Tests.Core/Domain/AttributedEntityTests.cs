using NetFusion.Tests.Core.Domain.Mocks;
using Xunit;
using FluentAssertions;

namespace NetFusion.Tests.Core.Domain
{
    public class AttributedEntityTests
    {
        [Fact]
        public void CanSetAndRetrieveEntityAttribute()
        {
            var entity = new DynamicEntity();
            entity.SetAttributeValue("Value1", 1000);

            var value = entity.Attributes.Value1 as object;
            value.Should().NotBeNull();
            value.Should().BeOfType<int>();

            var typedValue = (int)value;
            typedValue.Should().Be(1000);
        }

        [Fact]
        public void CanDeleteEntityAttribute()
        {
            var entity = new DynamicEntity();
            entity.SetAttributeValue("Value1", 1000);

            var isDeleted = entity.DeleteAttribute("Value1");
            isDeleted.Should().BeTrue();

            isDeleted = entity.DeleteAttribute("Value1");
            isDeleted.Should().BeFalse();
        }

        [Fact]
        public void CanCheckIfEntityAttributeExits()
        {
            var entity = new DynamicEntity();
            entity.SetAttributeValue("Value1", 1000);

            entity.ContainsAttribute("Value1").Should().BeTrue();
            entity.ContainsAttribute("Value2").Should().BeFalse();
        }

        [Fact]
        public void CanGetTypedEntityAttributeValue()
        {
            var entity = new DynamicEntity();
            entity.SetAttributeValue("Value1", 1000);

            var value = entity.GetAttributeValue<int>("Value1");
            value.Should().Be(1000);
        }

        [Fact]
        public void CanRequestDefaultValueIfEntityAttributeNotPresent()
        {
            var entity = new DynamicEntity();
            var value = entity.GetAttributeValueOrDefault<int>("Value1");
            value.Should().Be(0);
        }

        [Fact]
        public void CanRequestSpecificDefaultValueIfEntityAttributeNotPresent()
        {
            var entity = new DynamicEntity();
            var value = entity.GetAttributeValueOrDefault<int>("Value1", 1000);
            value.Should().Be(1000);
        }

        [Fact]
        public void IfEntityAttributePresentDefaultValueNotReturned()
        {
            var entity = new DynamicEntity();
            entity.Attributes.Value1 = 1000;
            var value = entity.GetAttributeValueOrDefault<int>("Value1", 2000);
            value.Should().Be(1000);
        }
    }
}

// TODO:  add method that takes a dictionary 
