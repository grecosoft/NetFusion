using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CommonTests.Extensions
{
    public class GenericTypeTests
    {
        [Fact (DisplayName = nameof(GivenType_DetermineOpenGeneric))]
        public void GivenType_DetermineOpenGeneric()
        {
            typeof(List<>).IsOpenGenericType().Should().BeTrue();
            typeof(List<int>).IsOpenGenericType().Should().BeFalse();
        }

        [Fact (DisplayName = nameof(GivenType_DetermineClosedGeneric))]
        public void GivenType_DetermineClosedGeneric()
        {
            var closedType = typeof(Tuple<int, string, int>);
            var openType = typeof(Tuple<,,>);

            closedType.IsClosedGenericTypeOf(openType).Should().BeTrue();

            openType = typeof(Tuple<,,,>);
            closedType.IsClosedGenericTypeOf(openType).Should().BeFalse();
        }

        [Fact (DisplayName = nameof(GivenType_DetermineClosedGenericOfParamTypes))]
        public void GivenType_DetermineClosedGenericOfParamTypes()
        {
            var closedType = typeof(Tuple<int, string, int>);
            var openType = typeof(Tuple<,,>);

            closedType.IsClosedGenericTypeOf(openType, 
                typeof(int), typeof(string), typeof(int))
                .Should().BeTrue();

            closedType.IsClosedGenericTypeOf(openType,
               typeof(string), typeof(string), typeof(int))
               .Should().BeFalse();
        }

        [Fact (DisplayName = nameof(GivenType_DetermineImplementedClosedInterfacesOfParamTypes))]
        public void GivenType_DetermineImplementedClosedInterfacesOfParamTypes()
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
