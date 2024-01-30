using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Common.UnitTests.Extensions.Reflection;

public class AttributeTests
{
    [Fact]
    public void GivenType_CanTestForAttribute()
    {
        typeof(TestClassWithAttribute).HasAttribute<TestAttribute>()
            .Should().BeTrue();

        typeof(TestClassWithAttribute).HasAttribute<ObsoleteAttribute>()
            .Should().BeFalse();
    }

    [Fact]
    public void GivenAttributeProvider_CanTestForAttribute()
    {
        typeof(TestClassWithAttribute).GetProperty("SomeProperty")!
            .HasAttribute<ObsoleteAttribute>()
            .Should().BeTrue();

        typeof(TestClassWithAttribute).GetProperty("SomeProperty")!
            .HasAttribute<TestAttribute>()
            .Should().BeFalse();
    }

    [Fact]
    public void GivenType_CanReferenceAttribute()
    {
        typeof(TestClassWithAttribute).GetAttribute<TestAttribute>()
            .Should().NotBeNull();

        typeof(TestClassWithAttribute).GetAttribute<ObsoleteAttribute>()
            .Should().BeNull();
    }

    [Fact]
    public void GivenAttributeProvider_CanReferenceAttribute()
    {
        typeof(TestClassWithAttribute).GetProperty("SomeProperty")!
            .GetAttribute<ObsoleteAttribute>()
            .Should().NotBeNull();

        typeof(TestClassWithAttribute).GetProperty("SomeProperty")!
            .GetAttribute<TestAttribute>()
            .Should().BeNull();
    }

    [Fact]
    public void GivenInstance_CanReferenceAttribute()
    {
        var instance = new TestClassWithAttribute();

        instance.GetAttribute<TestAttribute>()
            .Should().NotBeNull();

        instance.GetAttribute<ObsoleteAttribute>()
            .Should().BeNull();
    }

    [Fact]
    public void GivenType_CanHaveOnlyOne_MatchingAttribute()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            typeof(TestClassWithAttributes).GetAttribute<Attribute>();
        });

        ex.Message.Should().Contain("More than one attribute of the type");
    }

    [Fact]
    public void GivenAttributeProvider_CanHaveOnlyOne_MatchingAttribute()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            typeof(TestClassWithAttributes).GetProperty("SomeProperty")
                .GetAttribute<Attribute>();
        });
            
        ex.Message.Should().Contain("More than one attribute of the type");
    }

    [AttributeUsage(AttributeTargets.All)]
    private class TestAttribute : Attribute;
        
    [AttributeUsage(AttributeTargets.All)]
    private class Test2Attribute : Attribute;

    [Test]
    private class TestClassWithAttribute
    {
        [Obsolete]
        // ReSharper disable once UnusedMember.Local
        public string SomeProperty { get; set; }
    }

    [Test, Test2]
    private class TestClassWithAttributes
    {
        [Test, Test2]
        public string SomeProperty { get; set; }
    }
}