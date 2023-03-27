using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Collections;
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
    private IEnumerable<IMappingStrategy> MappingStrategies { get; set; }

    // SourceType ==> TargetMap(s)
    public ILookup<Type, TargetMap> SourceTypeMappings { get; private set; }

    public MappingModule()
    {
        StrategyFactories = Array.Empty<IMappingStrategyFactory>();
        MappingStrategies = Array.Empty<IMappingStrategy>();
        
        SourceTypeMappings = Enumerable.Empty<TargetMap>().ToLookup(m => m.SourceType);
    }
        
    // Finds all mapping strategies and caches the information for used at runtime by ObjectMapper.
    public override void Configure()
    {
        var factoryTargetMaps = StrategyFactories.SelectMany(f => f.GetStrategies())
            .Select(s => new TargetMap(s.SourceType, s.TargetType, s));

        var targetMaps = MappingStrategies.Select(s => new TargetMap(s.SourceType, s.TargetType, s.GetType()));

        SourceTypeMappings = targetMaps.Concat(factoryTargetMaps)
            .ToLookup(m => m.SourceType);
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
}