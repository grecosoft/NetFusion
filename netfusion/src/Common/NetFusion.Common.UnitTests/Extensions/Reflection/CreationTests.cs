using FluentAssertions;
using NetFusion.Common.Extensions.Reflection;

// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeMemberModifiers

namespace NetFusion.Common.UnitTests.Extensions.Reflection;

public class CreationTests
{
    [Fact]
    public void GivenType_CanDetermineIf_InstanceCanBeCreated()
    {
        typeof(TestTypeOne).IsCreatableClassType().Should().BeTrue();
        typeof(TestTypeTwo).IsCreatableClassType().Should().BeFalse();
    }

    [Fact]
    public void GivenType_CanCreateInstance()
    {
        var value = typeof(string).CreateInstance('a', 7);
        value.Should().NotBeNull();
        value.Should().Be(new string('a', 7));
    }

    [Fact]
    public void GivenListOfTypes_CanCreateInstances_DerivingFromType()
    {
        // Note:  Passing TestTypeOne twice to assure one instance is returned.
        var listOfTypes = new[] { typeof(TestTypeOne), typeof(TestTypeOne), typeof(TestTypeTwo), typeof(TestTypeThree) };
        var instances = listOfTypes.CreateInstancesDerivingFrom<IRunnable>().ToArray();

        instances.Should().HaveCount(2);
        instances.Where(i => i.GetType() == typeof(TestTypeOne)).Should().HaveCount(1);
        instances.Where(i => i.GetType() == typeof(TestTypeThree)).Should().HaveCount(1);

        var nonTypedInstances = listOfTypes.CreateInstancesDerivingFrom(typeof(IRunnable)).ToArray();
        nonTypedInstances.Should().HaveCount(2);
        nonTypedInstances.Where(i => i.GetType() == typeof(TestTypeOne)).Should().HaveCount(1);
        nonTypedInstances.Where(i => i.GetType() == typeof(TestTypeThree)).Should().HaveCount(1);
    }

    interface IRunnable;

    class TestTypeOne: IRunnable;

    class TestTypeTwo
    {
        public TestTypeTwo(int value){ }
    }

    class TestTypeThree : IRunnable;

}