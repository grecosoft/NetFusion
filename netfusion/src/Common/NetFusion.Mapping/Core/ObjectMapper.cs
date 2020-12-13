using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Mapping.Plugin;

// ReSharper disable ConvertIfStatementToNullCoalescingExpression

namespace NetFusion.Mapping.Core
{
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
            if (mappingModule == null) throw new ArgumentNullException(nameof(mappingModule));
            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            
            _sourceTypeMappings = mappingModule.SourceTypeMappings;
        }

        public TTarget Map<TTarget>(object source) where TTarget : class
        {
            if (! TryMap(source, out TTarget mappedResult))
            {
                throw new InvalidOperationException(
                    $"Mapping strategy not found.  Source: { source.GetType().FullName} Target: {typeof(TTarget).FullName}");
            }

            return mappedResult;
        }
        
        public bool TryMap<TTarget>(object source, out TTarget mappedResult)
            where TTarget : class
        {
            if (TryMap(source, typeof(TTarget), out object result))
            {
                mappedResult = (TTarget)result;
                return true;
            }

            mappedResult = null;
            return false;
        }

        public object Map(object source, Type targetType)
        {
            if (! TryMap(source, targetType, out object mappedResult))
            {
                throw new InvalidOperationException(
                    $"Mapping strategy not found.  Source: { source.GetType().FullName} Target: {targetType.FullName}");
            }

            return mappedResult;
        }
        
        public bool TryMap(object source, Type targetType, out object mappedResult)
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
                mappedResult = null;
                return false;
            }

            // If the mapping strategy instance was originally created by a IMappingStrategyFactory, 
            // return the cached instance.  Otherwise, create an instance of the custom mapping strategy
            // using the service provider.
            var strategy = targetMap.StrategyInstance ?? 
                           (IMappingStrategy)_services.GetRequiredService(targetMap.StrategyType);

            LogFoundMapping(targetMap);
            
            mappedResult = strategy.Map(this, source);
            if (mappedResult == null)
            {
                _logger.LogDebug(
                    "The mapping strategy for source: {sourceType} and target: {targetType} type returned null " + 
                    "for the mapped result.", source.GetType(), targetType);
            }
            
            return mappedResult != null;
        }

        // Determines if there is a mapping strategy matching the exact target type.  
        // If not present, a strategy with a matching target type, deriving from the 
        // specified target type, is searched.  This allows for polymorphic mapping
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
            _logger.LogDebug("Mapping Applied: {SourceType} --> {TargetType} Using Strategy: {StrategyType}", 
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
