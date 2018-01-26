using Autofac;
using NetFusion.Mapping;
using NetFusion.Mapping.Core;

namespace InfrastructureTests.Mapping.Setup
{
    public class ObjectMapperConfig
    {
        // Creates an instance of the ObjectMapper supplying an DI container that
        // would normally be provided by the runtime environment.
        public static IObjectMapper CreateObjectMapper(TargetMap[] targetMaps)
        {
            var builder = new ContainerBuilder();
            foreach (var map in targetMaps)
            {
                builder.RegisterType(map.StrategyType)
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }

            var module = new MockMappingModule(targetMaps);
            return new ObjectMapper(module, builder.Build());
        }
    }
}
