using System.Linq;
using System.Text;
using FluentAssertions;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Bootstrap
{
    public class ReflectionExtensionsTests
    {
        [Fact(DisplayName = nameof(CreateInstancesOfPluginTypes_MatchingSpecifiedOrBaseType))]
        public void CreateInstancesOfPluginTypes_MatchingSpecifiedOrBaseType()
        {
            var types = new[] {
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeOne), "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeTwo),  "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeTwo),  "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(StringBuilder),  "TestAssemblyName")
            };

            var objInstances = types.CreateInstancesDerivingFrom(typeof(ICommon)).ToArray();
            objInstances.Should().HaveCount(2);
            objInstances.OfType<TypeOne>().Should().HaveCount(1);
            objInstances.OfType<TypeTwo>().Should().HaveCount(1);

            var commonInstances = types.CreateInstancesDerivingFrom<ICommon>().ToArray();
            commonInstances.Should().HaveCount(2);
            commonInstances.OfType<TypeOne>().Should().HaveCount(1);
            commonInstances.OfType<TypeTwo>().Should().HaveCount(1);

            var typeTwoInstances = types.CreateInstancesDerivingFrom<TypeTwo>().ToArray();
            typeTwoInstances.Should().HaveCount(0);
            typeTwoInstances.Should().HaveCount(0);

            var stringInstances = types.CreateInstancesDerivingFrom<string>().ToArray();
            stringInstances.Should().HaveCount(0);
        }

        public interface ICommon { }
        public class TypeOne : ICommon { }
        public class TypeTwo : ICommon { }
    }
}
