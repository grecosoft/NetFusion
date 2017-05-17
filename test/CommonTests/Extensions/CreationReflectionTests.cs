using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;
using System.Linq;
using Xunit;

namespace CommonTests.Extensions
{
    public class CreationReflectionTests
    {
        [Fact (DisplayName = nameof(GivenType_CanDetermineCreatable))]
        public void GivenType_CanDetermineCreatable()
        {
            typeof(TestTypeOne).IsCreatableType().Should().BeTrue();
            typeof(TestTypeTwo).IsCreatableType().Should().BeFalse();
        }

        [Fact(DisplayName = nameof(GivenType_CanCreateInstance))]
        public void GivenType_CanCreateInstance()
        {
            var value = typeof(string).CreateInstance('a', 7);
            value.Should().NotBeNull();
            value.Should().Be(new string('a', 7));
        }

        [Fact(DisplayName = nameof(GivenListOfTypes_CanCreateInstances_DerivingFromType))]
        public void GivenListOfTypes_CanCreateInstances_DerivingFromType()
        {
            var listOfTypes = new[] { typeof(TestTypeOne), typeof(TestTypeTwo), typeof(TestTypeThree) };
            var instances = listOfTypes.CreateInstancesDerivingFrom<IRunnable>();

            instances.Should().HaveCount(2);
            instances.Where(i => i.GetType() == typeof(TestTypeOne)).Should().HaveCount(1);
            instances.Where(i => i.GetType() == typeof(TestTypeThree)).Should().HaveCount(1);

            var nonTypedInstances = listOfTypes.CreateInstancesDerivingFrom(typeof(IRunnable));
            nonTypedInstances.Should().HaveCount(2);
            nonTypedInstances.Where(i => i.GetType() == typeof(TestTypeOne)).Should().HaveCount(1);
            nonTypedInstances.Where(i => i.GetType() == typeof(TestTypeThree)).Should().HaveCount(1);
        }

        interface IRunnable { }

        class TestTypeOne: IRunnable { }

        class TestTypeTwo
        {
            public TestTypeTwo(int value){}
        }

        class TestTypeThree : IRunnable { }

    }
}
