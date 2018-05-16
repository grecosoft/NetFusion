﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable ConvertIfStatementToNullCoalescingExpression

namespace NetFusion.Mapping.Core
{
    /// <summary>
    /// Responsible for finding the mapping strategy for mapping a specified source type
    /// to its corresponding target type. 
    /// </summary>
    public class ObjectMapper : IObjectMapper
    {
        private readonly ILogger _logger;
        private readonly ILookup<Type, TargetMap> _sourceTypeMappings; // SourceType => TargetType(s)
        private readonly IServiceProvider _services;

        public ObjectMapper(
            ILoggerFactory loggerFactory,
            IMappingModule mappingModule,
            IServiceProvider services)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (mappingModule == null) throw new ArgumentNullException(nameof(mappingModule));
            
            _logger = loggerFactory.CreateLogger<ObjectMapper>();
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _sourceTypeMappings = mappingModule.SourceTypeMappings;
        }

        // Maps source object to the specified target or derived target object type.
        public TTarget Map<TTarget>(object source) where TTarget : class
        {
            if (source == null) throw new ArgumentNullException(nameof(source), 
                "Source object to map cannot be null.");

            object mappedObj = Map(source, typeof(TTarget));
            return (TTarget)mappedObj;           
        }

        public object Map(object source, Type targetType)
        {
            if (source == null) throw new ArgumentNullException(nameof(source),
                "Source object to map cannot be null.");

            if (targetType == null) throw new ArgumentNullException(nameof(targetType),
                "Target Type to map source to cannot be null.");

            TargetMap targetMap = FindMappingStrategy(source.GetType(), targetType);

            // Complete a reverse lookup.
            if (targetMap == null)
            {
                targetMap = FindMappingStrategy(targetType, source.GetType());
            }

            if (targetMap == null)
            {
                throw new InvalidOperationException(
                   $"Mapping strategy not found.  Source: { source.GetType().FullName} Target: {targetType.FullName}");
            }

            // If the mapping strategy instance was originally created by a IMappingStrategyFactory, 
            // return the cached instance.  Otherwise, create an instance of the custom mapping strategy
            // using the service provider..
            var strategy = targetMap.StrategyInstance ?? 
                           (IMappingStrategy)_services.GetRequiredService(targetMap.StrategyType);

            LogFoundMapping(targetMap);
            return strategy.Map(this, source);
        }

        // Determines if there is a mapping strategy matching the exact target type.  
        // If not present, a strategy with a matching target type, deriving from the 
        // specified target type, is searched.  This allows mapping polymorphic-ally 
        // to a derived target type for a corresponding source type.
        private TargetMap FindMappingStrategy(Type sourceType, Type targetType)
        {
            var sourceMappings = _sourceTypeMappings[sourceType].ToArray();
            if (! sourceMappings.Any())
            {
                // No source type registered mappings.
                return null;
            }

            var exactTargetMapping = AssertAndGetMapping(sourceType, targetType,
                sourceMappings.Where(tm => tm.TargetType == targetType));

            if (exactTargetMapping != null)
            {
                return exactTargetMapping;
            }

            // Search for a derived mapped type.  This allows mapping a given source into a derived type
            // specific to the source.
            var derivedTargetMapping = AssertAndGetMapping(sourceType, targetType,
                sourceMappings.Where(tm => tm.TargetType.IsDerivedFrom(targetType)));

            return derivedTargetMapping;
        }

        private void LogFoundMapping(TargetMap targetMap)
        {
            _logger.LogDebug(MappingLogEvents.MappingApplied, "Mapping Applied: {SourceType} --> {TargetType} Using Strategy: {StrategyType}", 
                targetMap.SourceType,
                targetMap.TargetType, 
                targetMap.StrategyType);
        }

        private static TargetMap AssertAndGetMapping(Type sourceType, Type targetType,  IEnumerable<TargetMap> targetMaps)
        {
            targetMaps = targetMaps.ToArray();

            if (targetMaps.Count() > 1)
            {
                throw new InvalidOperationException(
                   $"The source type of: {sourceType} has more than one mapping for the target type of: {targetType}");
            }
            return targetMaps.FirstOrDefault();
        }
    }
}
