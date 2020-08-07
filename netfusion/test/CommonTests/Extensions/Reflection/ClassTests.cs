using System;
using System.Linq;
using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using Xunit;
// ReSharper disable UnusedParameter.Local

namespace CommonTests.Extensions.Reflection
{
    public class ClassTests
    {
        [Fact]
        public void CanDetermine_ClassDerived_FromAnother()
        {
            typeof(ChildClassType).IsDerivedFrom(typeof(ParentClassType)).Should().BeTrue();
            typeof(ChildClassType).IsDerivedFrom<ParentClassType>().Should().BeTrue();
            typeof(ChildClassType).IsDerivedFrom<ChildClassType>().Should().BeFalse();
        }

        [Fact]
        public void CanDetermine_ClassAssignable_ToAnother()
        {
            typeof(ChildClassType).CanAssignTo<ParentClassType>().Should().BeTrue();
            typeof(ParentClassType).CanAssignTo(typeof(ChildClassType)).Should().BeFalse();
            typeof(ChildClassType).CanAssignTo<ChildClassType>().Should().BeTrue();
        }

        [Fact]
        public void CanDetermine_ConcreteClassDerived_FromAnother()
        {
            typeof(AbstractChildType).IsConcreteTypeDerivedFrom(typeof(ParentClassType)).Should().BeFalse();
            typeof(AbstractChildType).IsConcreteTypeDerivedFrom<ParentClassType>().Should().BeFalse();
            typeof(ChildClassType).IsConcreteTypeDerivedFrom<ParentClassType>().Should().BeTrue();
        }

        [Fact]
        public void CanDetermine_ClassDefines_DefaultConstructor()
        {
            typeof(NoDefaultConstructorClassType).HasDefaultConstructor().Should().BeFalse();
            typeof(ParentClassType).HasDefaultConstructor().Should().BeTrue();
        }

        [Fact]
        public void CanRequest_MethodParameterTypes()
        {
            var expectedParamTypes = new[] { typeof(byte[]), typeof(bool) };

            typeof(ChildClassType).GetMethod("SaveState", expectedParamTypes).
                GetParameterTypes()
                .Should()
                .BeEquivalentTo(expectedParamTypes.Select(t => t as object));
        }

        [Fact]
        public void CanRequest_ClassInterfaces_DerivedFromBaseInterface()
        {
            var foundInterfaces = typeof(ChildClassType).GetInterfacesDerivedFrom<IState>().ToArray();
            foundInterfaces.Should().HaveCount(1);
            foundInterfaces.First().Should().Be(typeof(IVersionedState));
            
            foundInterfaces = typeof(ChildClassType).GetInterfacesDerivedFrom<IVersionedState>().ToArray();
            foundInterfaces.Should().BeEmpty();
        }

        private class ParentClassType : IVersionedState
        {
            
        }

        private class ChildClassType : ParentClassType
        {
            public void SaveState(byte[] state, bool version)
            {
                
            }
        }

        private abstract class AbstractChildType : ParentClassType
        {
            
        }

        private class NoDefaultConstructorClassType
        {
            public NoDefaultConstructorClassType(int value)
            {
                throw new NotImplementedException();
            }
        }

        private interface IState
        {
            
        }

        private interface IVersionedState : IState
        {
            
        }
    }
}