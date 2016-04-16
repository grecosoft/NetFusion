using FluentAssertions;
using NetFusion.Common.Extensions;
using System;
using Xunit;

namespace NetFusion.Core.Tests.Common
{
    public class ReflectionUnitTests
    {
        [Fact]
        public void HasAttribute()
        {
            typeof(AttributeSource).HasAttribute<TestAttribute>()
                .Should().BeTrue();

            typeof(String).HasAttribute<TestAttribute>()
                .Should().BeFalse();
        }

        [Test]
        class AttributeSource
        {

        }

        class TestAttribute: Attribute
        {

        }
    }
}
