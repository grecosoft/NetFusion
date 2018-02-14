using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Mapping.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Mapping.Modules
{
    /// <summary>
    /// Plug-in module responsible for finding the mapping strategies to be applied at
    /// runtime to map a source objects to their corresponding target types.
    /// </summary>
    public class MappingModule : PluginModule,
        IMappingModule
    {
        // Discovered Properties:
        public IEnumerable<IMappingStrategyFactory> StrategyFactories { get; private set; }

        public ILookup<Type, TargetMap> SourceTypeMappings { get; private set; } // SourceType ==> TargetMap

        // Finds all mapping strategies and cache the information to be used
        // at runtime by ObjectMapper.
        public override void Configure()
        {
            var targetMappings = GetFactoryProvidedMappingStrategies()
                .Concat(GetCustomMappingStrategies());

            SourceTypeMappings = targetMappings.ToLookup(tm => tm.SourceType);             
        }

        // Finds all mappings provided by instances implementing IMappingStrategyFactory.
        // The strategies provided by a factory often are MappingDelegateStrategy 
        // instances that provides non-custom mapping and delegate to an open-source
        // library.
        private TargetMap[] GetFactoryProvidedMappingStrategies()
        {
            TargetMap[] targetMappings = StrategyFactories
                .SelectMany(f => f.GetStrategies())
                .Select(s => new TargetMap
                {
                    StrategyInstance = s,
                    SourceType = s.SourceType,
                    TargetType = s.TargetType
                }).ToArray();

            return targetMappings;
        }

        // Find all types that are a closed type of IMappingStrategy<,> such 
        // as IMappingStrategy<Car, CarModel>.  These are mapping strategies
        // containing custom mapping logic.  These mapping strategies are
        // registered in the DI container and therefore can have dependencies 
        // injected. 
        private TargetMap[] GetCustomMappingStrategies()
        {
            Type openGenericMapType = typeof(IMappingStrategy<,>);

            TargetMap[] targetMappings = Context.AllPluginTypes
                .WhereHavingClosedInterfaceTypeOf(openGenericMapType)
                .Select(ti => new TargetMap
                {
                    StrategyType = ti.Type,
                    SourceType = ti.GenericArguments[0],
                    TargetType = ti.GenericArguments[1]
                }).ToArray();

            return targetMappings;
        }
        
        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Service used as the central entry point for mapping objects.
            builder.RegisterType<ObjectMapper>()
                .As<IObjectMapper>()
                .InstancePerLifetimeScope();

            // Register the custom mappings with the container.  This will allow mappings 
            // to inject any services needed to complete the mapping.
            foreach (TargetMap map in SourceTypeMappings.Values().Where(
                map => map.StrategyInstance == null))
            {
                builder.RegisterType(map.StrategyType)
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
