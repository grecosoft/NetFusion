﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Services.Mapping.Plugin;

namespace NetFusion.Services.Mapping.Core;

/// <summary>
/// Responsible for finding the mapping strategy for mapping a specified source type
/// to its corresponding target type.   If a matching strategy is found, it is invoked
/// to map the source type to its corresponding target type.
/// </summary>
public class ObjectMapper : IObjectMapper
{
    private readonly ILogger _logger;
    private readonly ILookup<Type, TargetMap> _sourceTypeMappings; // SourceType => TargetType(s)
    private readonly IServiceProvider _services;

    public ObjectMapper(
        ILogger<ObjectMapper> logger,
        IMappingModule mappingModule,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(mappingModule);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _services = services ?? throw new ArgumentNullException(nameof(services));
            
        _sourceTypeMappings = mappingModule.SourceTypeMappings;
    }

    public TTarget Map<TTarget>(object source) where TTarget : class
    {
        if (! TryMap(source, out TTarget? mappedResult))
        {
            throw new MappingException(
                $"Mapping strategy not found.  Source: { source.GetType().FullName} Target: {typeof(TTarget).FullName}",
                "MAPPING_NOT_FOUND");
        }

        return mappedResult;
    }
        
    public bool TryMap<TTarget>(object source, [NotNullWhen(true)]out TTarget? mappedResult)
        where TTarget : class
    {
        if (TryMap(source, typeof(TTarget), out object? result))
        {
            mappedResult = (TTarget)result;
            return true;
        }

        mappedResult = null;
        return false;
    }

    public object Map(object source, Type targetType)
    {
        if (! TryMap(source, targetType, out object? mappedResult))
        {
            throw new MappingException(
                $"Mapping strategy not found. Source: { source.GetType().FullName} Target: {targetType.FullName}", 
                "MAPPING_NOT_FOUND");
        }

        return mappedResult;
    }
        
    public bool TryMap(object source, Type targetType, [NotNullWhen(true)] out object? mappedResult)
    {
        if (source == null) throw new ArgumentNullException(nameof(source),
            "Source object to map cannot be null.");

        if (targetType == null) throw new ArgumentNullException(nameof(targetType),
            "Target Type to map source to cannot be null.");

        // Find mapping strategy for mapping between the source and target types.
        TargetMap? targetMap = FindTargetMap(source.GetType(), targetType); 

        if (targetMap == null)
        {
            mappedResult = null;
            return false;
        }

        // If the mapping strategy instance was originally created by a IMappingStrategyFactory, 
        // return the cached instance.  Otherwise, create an instance of the custom mapping strategy
        // using the service provider.
        var strategy = targetMap.StrategyInstance ?? 
                       (IMappingStrategy)_services.GetRequiredService(targetMap.StrategyType!);

        LogFoundMapping(targetMap);
            
        mappedResult = strategy.Map(this, source);
        return true;
    }

    // Determines if there is a mapping strategy matching the exact target type.  
    // If not present, a strategy with a matching target type, deriving from the 
    // specified target type, is searched.  This allows for polymorphic mapping
    // to a derived target type for a corresponding source type.
    private TargetMap? FindTargetMap(Type sourceType, Type targetType)
    {
        var sourceMappings = _sourceTypeMappings[sourceType].ToArray();
        if (sourceMappings.Length == 0)
        {
            // No source type registered mappings.
            return null;
        }

        var exactTargetMapping = GetTargetMap(sourceType, targetType,
            sourceMappings.Where(tm => tm.TargetType == targetType));

        if (exactTargetMapping != null)
        {
            return exactTargetMapping;
        }

        var derivedTargetMapping = GetTargetMap(sourceType, targetType,
            sourceMappings.Where(tm => tm.TargetType.IsDerivedFrom(targetType)));

        return derivedTargetMapping;
    }
    
    private static TargetMap? GetTargetMap(Type sourceType, Type targetType, IEnumerable<TargetMap> targetMaps)
    {
        var mappings = targetMaps as TargetMap[] ?? targetMaps.ToArray();
        if (mappings.Length > 1)
        {
            throw new MappingException(
                $"The source type of: {sourceType} has more than one mapping for the target type of: {targetType}",
                "MULTIPLE_MAPPINGS_FOUND");
        }
        return mappings.FirstOrDefault();
    }

    private void LogFoundMapping(TargetMap targetMap)
    {
        _logger.LogDebug("Mapping Applied: {SourceType} --> {TargetType} Using Strategy: {StrategyType}", 
            targetMap.SourceType,
            targetMap.TargetType, 
            targetMap.StrategyType ?? targetMap.StrategyInstance?.GetType());
    }
}