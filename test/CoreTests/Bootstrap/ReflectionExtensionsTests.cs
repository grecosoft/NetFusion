using FluentAssertions;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Test.Plugins;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace BootstrapTests.Bootstrap
{
    public class ReflectionExtensionsTests
    {
        [Fact(DisplayName = nameof(FilterInstances_BasedOnListOfTypes))]
        public void FilterInstances_BasedOnListOfTypes()
        {
            var instances = new ICommon[] { new TypeOne(), new TypeTwo() };
            instances.CreatedFrom(new[] { typeof(string) })
                .Should()
                .HaveCount(0);

            instances.CreatedFrom(new[] { typeof(TypeTwo) })
               .Should()
               .HaveCount(1);

            instances.CreatedFrom(new[] { typeof(TypeOne), typeof(TypeTwo), typeof(TypeTwo) })
              .Should()
              .HaveCount(2);
        }

        [Fact(DisplayName = nameof(FilterInstances_BasedOnListOfPluginTypes))]
        public void FilterInstances_BasedOnListOfPluginTypes()
        {
            var instances = new ICommon[] { new TypeOne(), new TypeTwo() };
            instances.CreatedFrom(new[] { new PluginType(new Plugin(new MockCorePlugin()), typeof(string), "TestAssemblyName") })
                .Should()
                .HaveCount(0);

            instances.CreatedFrom(new[] { new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeTwo), "TestAssemblyName") } )
               .Should()
               .HaveCount(1);

            instances.CreatedFrom(new[] {
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeOne),  "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeTwo),  "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeTwo),  "TestAssemblyName") })
                .Should()
                .HaveCount(2);
        }

        [Fact(DisplayName = nameof(CreateInstancesOfPluginTypes_MatchingSpecifiedOrBaseType))]
        public void CreateInstancesOfPluginTypes_MatchingSpecifiedOrBaseType()
        {
            var types = new[] {
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeOne), "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeTwo),  "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(TypeTwo),  "TestAssemblyName"),
                new PluginType(new Plugin(new MockCorePlugin()), typeof(StringBuilder),  "TestAssemblyName")
            };

            var instances = types.CreateInstancesDerivingFrom(typeof(ICommon));
            instances.Should().HaveCount(2);
            instances.OfType<TypeOne>().Should().HaveCount(1);
            instances.OfType<TypeTwo>().Should().HaveCount(1);

            instances = types.CreateInstancesDerivingFrom<ICommon>();
            instances.Should().HaveCount(2);
            instances.OfType<TypeOne>().Should().HaveCount(1);
            instances.OfType<TypeTwo>().Should().HaveCount(1);

            instances = types.CreateInstancesDerivingFrom<TypeTwo>();
            instances.Should().HaveCount(0);
            instances.OfType<TypeTwo>().Should().HaveCount(0);

            instances = types.CreateInstancesDerivingFrom<string>();
            instances.Should().HaveCount(0);


        }

        public interface ICommon { }
        public class TypeOne : ICommon { }
        public class TypeTwo : ICommon { }
    }
}
