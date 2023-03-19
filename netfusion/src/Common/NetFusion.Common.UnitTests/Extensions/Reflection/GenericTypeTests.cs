using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Common.UnitTests.Extensions.Reflection;

public class GenericTypeTests
{
    [Fact]
    public void GivenType_CanDetermineOpenGeneric()
    {
        typeof(List<>).IsOpenGenericType().Should().BeTrue();
        typeof(List<int>).IsOpenGenericType().Should().BeFalse();
    }

    [Fact]
    public void GivenType_CanDetermineClosedGeneric()
    {
        var closedType = typeof(Tuple<int, string, int>);
        var openType = typeof(Tuple<,,>);

        closedType.IsClosedGenericTypeOf(openType).Should().BeTrue();

        openType = typeof(Tuple<,,,>);
        closedType.IsClosedGenericTypeOf(openType).Should().BeFalse();
    }

    [Fact]
    public void GivenType_CanDetermineClosedGenericOfAssignableParamTypes()
    {
        var closedType = typeof(Tuple<int, string, int>);
        var openType = typeof(Tuple<,,>);

        closedType.IsClosedGenericTypeOf(openType, 
                typeof(int), typeof(string), typeof(int))
            .Should().BeTrue();

        closedType.IsClosedGenericTypeOf(openType,
                typeof(string), typeof(string), typeof(int))
            .Should().BeFalse();

        closedType.IsClosedGenericTypeOf(openType,
                typeof(object), typeof(string), typeof(object))
            .Should().BeTrue();
    }

    [Fact]
    public void GivenTypes_CanDetermineThoseImplementingClosedGenericInterfacesOfAssignableParamTypes()
    {
        var closedTypes = new[] { typeof(Dictionary<string, int>), typeof(List<int>) };
        var matchingTypes = closedTypes.WhereHavingClosedInterfaceTypeOf(
                typeof(IDictionary<,>), typeof(string), typeof(object))
            .ToArray();

        matchingTypes.Should().HaveCount(1);
        var genericInfo = matchingTypes.First();

        genericInfo.GenericArguments.Should().HaveCount(2);
        genericInfo.GenericArguments[0].Should().BeSameAs(typeof(string));
        genericInfo.GenericArguments[1].Should().BeSameAs(typeof(int));
    }

    [Fact]
    public void GivenListOfTypes_CanDetermineThoseImplementingClosedInterfaceOfAssignableParamTypes()
    {
        var types = new[] { typeof(ItemOne), typeof(ItemTwo), typeof(ItemThree) };
            
        types.WhereHavingClosedInterfaceTypeOf(typeof(ITaggedItem<>), typeof(object))
            .Should().HaveCount(3);
            
        types.WhereHavingClosedInterfaceTypeOf(typeof(ITaggedItem<>), typeof(string))
            .Should().HaveCount(2);

        types.WhereHavingClosedInterfaceTypeOf(typeof(ITaggedItem<>), typeof(int))
            .Should().HaveCount(1);

        types.WhereHavingClosedInterfaceTypeOf(typeof(ITaggedItem<>), typeof(decimal))
            .Should().BeEmpty();
    }
        
    private interface ITaggedItem<T>
    {
        T Tag { get; set; }
    }

    public class ItemOne : ITaggedItem<int>
    {
        public int Tag { get; set; }
    }

    public class ItemTwo : ITaggedItem<string>
    {
        public string Tag { get; set; }
    }
        
    public class ItemThree : ITaggedItem<string>
    {
        public string Tag { get; set; }
    }
}