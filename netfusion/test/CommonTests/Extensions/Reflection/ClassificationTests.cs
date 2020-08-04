using System;
using System.Text;
using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using Xunit;

namespace CommonTests.Extensions.Reflection
{
    public class ClassificationTests
    {
        [Fact]
        public void CanDetermine_CategoryOfType()
        {
            typeof(int).IsBasicType().Should().BeTrue();
            typeof(int?).IsBasicType().Should().BeTrue();
            typeof(DateTime).IsBasicType().Should().BeTrue();
            typeof(string).IsBasicType().Should().BeTrue();
            new StringBuilder().GetType().IsBasicType().Should().BeFalse();
            typeof(double).IsBasicType().Should().BeTrue();
            typeof(decimal).IsBasicType().Should().BeTrue();
            typeof(decimal?).IsBasicType().Should().BeTrue();
        }
    }
}