using Autofac;
using NetFusion.Common;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Utilities.Mapping;
using System;
using System.Linq;

namespace NetFusion.Utilities.Core
{
    /// <summary>
    /// Responsible for finding the mapping strategy for mapping a specified source type
    /// to its corresponding target type. If no matching strategy is found, the source 
    /// is automatically mapped to an instance of the target type using the client provided
    /// IAutoMapper instance.
    /// </summary>
    public class ObjectMapper : IObjectMapper
    {
        private static ILookup<Type, TargetMap> _sourceTypeMappings;
        private static IAutoMapper _autoMapper;
        private readonly ILifetimeScope _lifetimeScope;

        public ObjectMapper(
            IMappingModule mappingModule,
            ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
            _sourceTypeMappings = mappingModule.SourceTypeMappings;
            _autoMapper = mappingModule.AutoMapper;
        }

        // Maps source object to the specified target or derived target object type.
        public TTarget Map<TTarget>(object source)
            where TTarget : class
        {
            Check.NotNull(source, nameof(source));

            IMappingStrategy strategy = null;
            TargetMap targetMap = FindMappingStrategy(source.GetType(), typeof(TTarget));

            // Complete a reverse lookup.
            if (targetMap == null)
            {
                targetMap = FindMappingStrategy(typeof(TTarget), source.GetType());
            }

            // If mapping strategy found, create an instance using the container.  This allows
            // the strategy to inject any required services required to complete the mappings.
            if (targetMap != null)
            {
                strategy = (IMappingStrategy)_lifetimeScope.Resolve(targetMap.StrategyType);
                return (TTarget)strategy.Map(_autoMapper, source);
            }

            // If there is no corresponding mapping strategy delegate the registered IAutoMapper instance.
            return _autoMapper.Map<TTarget>(source);
        }

        // Determines if there is a mapping strategy matching the exact target type.  
        // If not present, a strategy with a matching target type deriving from the 
        // specified target type is searched.
        private TargetMap FindMappingStrategy(Type sourceType, Type targetType)
        {
            var sourceMappings = _sourceTypeMappings[sourceType];
            if (!sourceMappings.Any())
            {
                return null;
            }

            var exactTargetMapping = sourceMappings.FirstOrDefault(tm => tm.TargetType == targetType);
            if (exactTargetMapping != null)
            {
                return exactTargetMapping;
            }

            // Search for a derived mapped type.  This allows mapping a given source into a derived type
            // specific to the source.
            var derivedTargetMappings = sourceMappings.Where(tm => tm.TargetType.IsDerivedFrom(targetType));
            if (derivedTargetMappings.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"The source type of: {sourceType} has more than one mapping for the target type of: {targetType}");
            }

            if (derivedTargetMappings.Any())
            {
                return derivedTargetMappings.First();
            }

            return null;
        }
    }
}
