using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using System;
using Xunit;
using System.Reflection;

namespace CommonTests.Extensions
{
    public class AttributeTests
    {
        [Fact(DisplayName = nameof(GivenType_CanTestForAttribute))]
        public void GivenType_CanTestForAttribute()
        {
            typeof(TestClassWithAttribute).HasAttribute<TestAttribute>()
                .Should().BeTrue();

            typeof(TestClassWithAttribute).HasAttribute<ObsoleteAttribute>()
                .Should().BeFalse();
        }

        [Fact(DisplayName = nameof(GivenAttributeProvider_CanTestForAttribute))]
        public void GivenAttributeProvider_CanTestForAttribute()
        {
            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
                .HasAttribute<ObsoleteAttribute>()
                .Should().BeTrue();

            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
                .HasAttribute<TestAttribute>()
                .Should().BeFalse();
        }

        [Fact(DisplayName = nameof(GivenType_CanReferenceAttribute))]
        public void GivenType_CanReferenceAttribute()
        {
            typeof(TestClassWithAttribute).GetAttribute<TestAttribute>()
                .Should().NotBeNull();

            typeof(TestClassWithAttribute).GetAttribute<ObsoleteAttribute>()
                .Should().BeNull();
        }

        [Fact(DisplayName = nameof(GivenAttributePrivider_CanReferenceAttribute))]
        public void GivenAttributePrivider_CanReferenceAttribute()
        {
            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
                .GetAttribute<ObsoleteAttribute>()
                .Should().NotBeNull();

            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
               .GetAttribute<TestAttribute>()
               .Should().BeNull();
        }

        [Fact(DisplayName = nameof(GivenInstance_CanReferenceAttribute))]
        public void GivenInstance_CanReferenceAttribute()
        {
            var instance = new TestClassWithAttribute();

            instance.GetAttribute<TestAttribute>()
                .Should().NotBeNull();

            instance.GetAttribute<ObsoleteAttribute>()
                .Should().BeNull();
        }

        class TestAttribute : Attribute { }

        [Test]
        class TestClassWithAttribute
        {
            [Obsolete]
            public string SomeProperty { get; set; }
        }
    }
}
