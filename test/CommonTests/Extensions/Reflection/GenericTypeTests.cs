using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CommonTests.Extensions.Reflection
{
    public class GenericTypeTests
    {
        [Fact (DisplayName = "Given Type can determine Open Generic")]
        public void GivenType_CanDetermineOpenGeneric()
        {
            typeof(List<>).IsOpenGenericType().Should().BeTrue();
            typeof(List<int>).IsOpenGenericType().Should().BeFalse();
        }

        [Fact (DisplayName = "Gen Type can determine Closed Generic")]
        public void GivenType_CanDetermineClosedGeneric()
        {
            var closedType = typeof(Tuple<int, string, int>);
            var openType = typeof(Tuple<,,>);

            closedType.IsClosedGenericTypeOf(openType).Should().BeTrue();

            openType = typeof(Tuple<,,,>);
            closedType.IsClosedGenericTypeOf(openType).Should().BeFalse();
        }

        [Fact (DisplayName = "Given Type can determine Close Generic of Assignable Param. Types")]
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

        [Fact (DisplayName = "Given Types can determine those Implementing Closed Generic Interface of Assignable Param. Types.")]
        public void GivenTypes_CanDetermineThoseImplementingClosedGenericInterfacesOfAssignableParamTypes()
        {
            var closedTypes = new[] { typeof(Dictionary<string, int>), typeof(List<int>) };
            var matchingTypes = closedTypes.WhereHavingClosedInterfaceTypeOf(
                typeof(IDictionary<,>), typeof(string), typeof(object));

            matchingTypes.Should().HaveCount(1);
            var genericInfo = matchingTypes.First();

            genericInfo.GenericArguments.Should().HaveCount(2);
            genericInfo.GenericArguments[0].Should().BeSameAs(typeof(string));
            genericInfo.GenericArguments[1].Should().BeSameAs(typeof(int));
        }
    }
}
