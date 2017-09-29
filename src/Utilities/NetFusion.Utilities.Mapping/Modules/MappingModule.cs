using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Utilities.Core;
using NetFusion.Utilities.Mapping;
using NetFusion.Utilities.Mapping.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Utilities.Modules
{
    public class MappingModule : PluginModule,
        IMappingModule
    {
        public IEnumerable<IMappingStrategyFactory> StrategyFactories { get; private set; }

        public ILookup<Type, TargetMap> SourceTypeMappings { get; private set; }
        public IAutoMapper AutoMapper { get; private set; }

        // Find all types that are a closed type of IMappingStrategy<,> such 
        // as IMappingStrategy<Car, CarModel> and creates a lookup keyed on
        // the source type.  This cached information will be used at runtime
        // by the ObjectMapper.
        public override void Configure()
        {
            var mappingConfig = this.Context.Plugin.GetConfig<MappingConfig>();

            // Create an instance of IAutoMapper specified by the consuming application.
            // The implementation will most likely delegate to an open source mapping
            // library.  This decouples the plug-in from any specific mapping library.
            this.AutoMapper = (IAutoMapper)mappingConfig.AutoMapperType.CreateInstance();

            var targetMappings = GetFactoryProvidedMappingStrategies()
                .Concat(GetCustomMappingStrategies());

            this.SourceTypeMappings = targetMappings.ToLookup(tm => tm.SourceType);             
        }

        // Finds all mappings provided by instances implementing IMappingStrategyFactory.
        // The strategies provided by a factory are usually MappingDelegateStrategy 
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
        // containing custom mapping logic.
        private TargetMap[] GetCustomMappingStrategies()
        {
            Type openGenericMapType = typeof(IMappingStrategy<,>);

            TargetMap[] targetMappings = this.Context.AllPluginTypes
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
            builder.RegisterType<ObjectMapper>()
                .As<IObjectMapper>()
                .InstancePerLifetimeScope();

            // Register the mappings with the container.  This will allow mappings to inject
            // any services needed to complete the mapping.
            foreach (TargetMap map in this.SourceTypeMappings.Values())
            {
                if (map.StrategyInstance != null)
                {
                    continue;
                }

                builder.RegisterType(map.StrategyType)
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
