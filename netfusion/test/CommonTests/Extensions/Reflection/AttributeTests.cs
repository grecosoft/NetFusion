using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using System;
using Xunit;

namespace CommonTests.Extensions.Reflection
{
    public class AttributeTests
    {
        [Fact(DisplayName = "Given Type can test for Attribute")]
        public void GivenType_CanTestForAttribute()
        {
            typeof(TestClassWithAttribute).HasAttribute<TestAttribute>()
                .Should().BeTrue();

            typeof(TestClassWithAttribute).HasAttribute<ObsoleteAttribute>()
                .Should().BeFalse();
        }

        [Fact(DisplayName = "Given Attribute Provider can test for Attribute")]
        public void GivenAttributeProvider_CanTestForAttribute()
        {
            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
                .HasAttribute<ObsoleteAttribute>()
                .Should().BeTrue();

            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
                .HasAttribute<TestAttribute>()
                .Should().BeFalse();
        }

        [Fact(DisplayName = "Given Type can reference Attribute")]
        public void GivenType_CanReferenceAttribute()
        {
            typeof(TestClassWithAttribute).GetAttribute<TestAttribute>()
                .Should().NotBeNull();

            typeof(TestClassWithAttribute).GetAttribute<ObsoleteAttribute>()
                .Should().BeNull();
        }

        [Fact(DisplayName = "Given Attribute Provider can reference Attribute")]
        public void GivenAttributeProvider_CanReferenceAttribute()
        {
            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
                .GetAttribute<ObsoleteAttribute>()
                .Should().NotBeNull();

            typeof(TestClassWithAttribute).GetProperty("SomeProperty")
               .GetAttribute<TestAttribute>()
               .Should().BeNull();
        }

        [Fact(DisplayName = "Given Instance can reference Attribute")]
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
            // ReSharper disable once UnusedMember.Local
            public string SomeProperty { get; set; }
        }
    }
}
