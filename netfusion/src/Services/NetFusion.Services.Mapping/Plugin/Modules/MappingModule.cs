using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Services.Mapping.Core;

namespace NetFusion.Services.Mapping.Plugin.Modules;

/// <summary>
/// Plug-in module responsible for finding the mapping strategies to be applied at
/// runtime to map source objects to their corresponding target types.
/// </summary>
public class MappingModule : PluginModule,
    IMappingModule
{
    // Discovered Properties:
    private IEnumerable<IMappingStrategyFactory> StrategyFactories { get; set; }

    // SourceType ==> TargetMap(s)
    public ILookup<Type, TargetMap> SourceTypeMappings { get; private set; }

    public MappingModule()
    {
        StrategyFactories = Array.Empty<IMappingStrategyFactory>();
        SourceTypeMappings = Enumerable.Empty<TargetMap>().ToLookup(m => m.SourceType);
    }

    // Finds all mapping strategies and caches the information for used at runtime by ObjectMapper.
    public override void Configure()
    {
        var targetMappings = GetFactoryProvidedMappingStrategies()
            .Concat(GetCustomMappingStrategies());

        SourceTypeMappings = targetMappings.ToLookup(tm => tm.SourceType);  
    }
    
    // All mappings provided by instances implementing IMappingStrategyFactory.
    private IEnumerable<TargetMap> GetFactoryProvidedMappingStrategies() => StrategyFactories
        .SelectMany(f => f.GetStrategies())
        .Select(s => new TargetMap(s.SourceType, s.TargetType, s));

    // Find all types that are a closed type of IMappingStrategy<,> such as IMappingStrategy<Car, CarModel>.
    // These are mapping strategies containing custom mapping logic.  These mapping strategies are registered
    // in the DI container and therefore can have dependencies injected. 
    private IEnumerable<TargetMap> GetCustomMappingStrategies()
    {
        Type openGenericMapType = typeof(IMappingStrategy<,>);

        return Context.AllPluginTypes
            .WhereHavingClosedInterfaceTypeOf(openGenericMapType)
            .Where(pt => !pt.Type.IsAbstract && !pt.Type.IsGenericType)
            .Select(ti => new TargetMap(ti.GenericArguments[0], ti.GenericArguments[1], ti.Type));
    }
    
    public override void RegisterServices(IServiceCollection services)
    {
        // Service used as the central entry point for mapping objects.
        services.AddScoped<IObjectMapper, ObjectMapper>();  
        
        // Register all non-factory provided mappings with the container.  This will
        // allow mappings to inject any services needed to complete the mapping.
        var strategyTypesToRegister = SourceTypeMappings.Values()
            .Where(m => m.StrategyType != null)
            .Select(m => m.StrategyType);

        foreach (var strategyType in strategyTypesToRegister)
        {
            services.AddScoped(strategyType!);
        }
    }

    public override void Log(IDictionary<string, object> moduleLog)
    {
        moduleLog["mappings"] = SourceTypeMappings.Values().Select(m => new
        {
            m.SourceType,
            m.TargetType,
            StrategyType = m.StrategyType ?? m.StrategyInstance?.GetType()
        });
    }
}