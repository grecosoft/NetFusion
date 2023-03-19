using System.Text;
using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Common.UnitTests.Extensions.Reflection;

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

    [Fact]
    public void CanDetermine_Category_OfPropertyType()
    {
        typeof(TestClass).GetProperty("PropOne").IsBasicType().Should().BeFalse();
        typeof(TestClass).GetProperty("PropTwo").IsBasicType().Should().BeTrue();
    }

    private class TestClass
    {
        public object PropOne { get; set; }
        public int? PropTwo { get; set; }
    }
}