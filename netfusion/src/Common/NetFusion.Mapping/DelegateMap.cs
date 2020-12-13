using System;

namespace NetFusion.Mapping
{
    /// <summary>
    /// Static class used to create an instance of generic DelegateMap class for a
    /// specific source and target types.  This allows the instance to be created
    /// based on the mapping function parameter types. 
    /// </summary>
    public static class DelegateMap
    {
        /// <summary>
        /// Called to specify a delegate for one way mapping between a source and target type.
        /// </summary>
        /// <param name="sourceToTarget">Function for mapping between the source and target type.</param>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <returns>Instance of a DelegateMap used to convert between a source and target type.</returns>
        public static DelegateMap<TSource, TTarget> Map<TSource, TTarget>(
            Func<TSource, TTarget> sourceToTarget)
        
            where TSource: class 
            where TTarget: class
        {
            var map = new DelegateMap<TSource, TTarget>();
            map.SetMap(sourceToTarget);

            return map;
        }

        /// <summary>
        /// Called to specify delegates for converting between a source and target
        /// types in both directions.
        /// </summary>
        /// <param name="sourceToTarget">Function for mapping between the source and target type.</param>
        /// <param name="targetToSource">Function for mapping between the target and source type.</param>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <returns>Instance of a DelegateMap used to convert back and fourth between a source
        /// and target type.</returns>
        public static DelegateMap<TSource, TTarget> Map<TSource, TTarget>(
            Func<TSource, TTarget> sourceToTarget, 
            Func<TTarget, TSource> targetToSource)
        
            where TSource : class
            where TTarget : class
        {
            var map = new DelegateMap<TSource, TTarget>();
            map.SetMap(sourceToTarget);
            map.SetMap(targetToSource);

            return map;
        }
    }
    
    /// <summary>
    /// A derived mapping strategy specified as function delegates for mapping between a source
    /// and target type.  An instance of this class is created by invoking methods on the static
    /// DelegateMap class.  This provides a compact way of defining mappings when implementing a
    /// IMappingStrategyFactory.  The factory implementation can "yield return" multiple DelegateMap
    /// instances for mapping between a set of types.  For more complex mappings between a specific
    /// source and target types, a strategy can be derived directly from the generic MappingStrategy
    /// class.
    /// </summary>
    /// <typeparam name="TSource">The source type to be mapped.</typeparam>
    /// <typeparam name="TTarget">The target type to be mapped into.</typeparam>
    public sealed class DelegateMap<TSource, TTarget> : MappingStrategy<TSource, TTarget> 
        where TSource : class
        where TTarget : class
    {
        private Func<TSource, TTarget> _sourceToTarget = s => null;
        private Func<TTarget, TSource> _targetToSource = t => null;
        
        internal void SetMap(Func<TSource, TTarget> map)
        {
            _sourceToTarget = map ?? throw new ArgumentNullException(nameof(map));
        }
        
        internal void SetMap(Func<TTarget, TSource> map)
        {
            _targetToSource = map ?? throw new ArgumentNullException(nameof(map));
        }

        // Base method overrides implemented by delegating to the specified
        // mapping delegates.
        
        protected override TTarget SourceToTarget(TSource source) => _sourceToTarget(source);

        protected override TSource TargetToSource(TTarget target) => _targetToSource(target);
    }
}