using FluentAssertions;
using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Xunit;
using TypeExtensions = NetFusion.Common.Extensions.TypeExtensions;

namespace NetFusion.Core.Tests.Common
{
    public class TypeExtensionTests
    {

        [Fact]
        public void CreateInstanceOfGenericType_ReturnsInstance()
        {
            var value = TypeExtensions.CreateInstance<string>('a', 7);
            value.Should().NotBeNull();
            value.Should().Be(new string('a', 7));
        }

        [Fact]
        public void CreateInstanceOfRuntimeType_ReturnsInstance()
        {
            var value = TypeExtensions.CreateInstance(typeof(string), 'a', 7);
            value.Should().NotBeNull();
            value.Should().Be(new string('a', 7));
        }

        [Fact]
        public void ChildImplementingInterfaceType_ReturnsTrue()
        {
            typeof(StringBuilder).IsDerivedFrom<ISerializable>()
                .Should().BeTrue();
        }

        [Fact]
        public void ChildImplementingRuntimeInterfaceType_ReturnsTrue()
        {
            typeof(StringBuilder).IsDerivedFrom(typeof(ISerializable))
                .Should().BeTrue();
        }

        [Fact]
        public void ChildDerivedFromParentType_ReturnsTrue()
        {
            typeof(StringBuilder).IsDerivedFrom<object>()
                .Should().BeTrue();
        }

        [Fact]
        public void ChildDerivedFromParentRuntimeType_ReturnsTrue()
        {
            typeof(StringBuilder).IsDerivedFrom(typeof(object))
                .Should().BeTrue();
        }

        [Fact]
        public void IsOpenGenericType()
        {
            typeof(List<>).IsOpenGenericType().Should().BeTrue();
            typeof(List<int>).IsOpenGenericType().Should().BeFalse();
        }

        [Fact]
        public void IsClosedGenericTypeOf()
        {
            var closedType = typeof(Tuple<int, string, int>);
            var openType = typeof(Tuple<,,>);

            closedType.IsClosedGenericTypeOf(openType).Should().BeTrue();
        }

        [Fact]
        public void IsClosedGenericTypeOfWithSpecificParamTypes()
        {

            var closedType = typeof(Tuple<int, string, int>);
            var openType = typeof(Tuple<,,>);

            closedType.IsClosedGenericTypeOf(openType, 
                typeof(int), typeof(string), typeof(int))
                .Should().BeTrue();
        }

        [Fact]
        public void WhereImplementsClosedInterfaceTypeOf()
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
