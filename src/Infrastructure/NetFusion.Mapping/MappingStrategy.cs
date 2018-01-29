using System;

namespace NetFusion.Mapping
{
    /// <summary>
    /// Base class used to define mapping strategies between source and target types.
    /// </summary>
    public abstract class MappingStrategy : IMappingStrategy
    {
        public Type SourceType { get; }
        public Type TargetType { get; }

        protected MappingStrategy(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        public abstract object Map(IObjectMapper mapper, object obj);
    }

    /// <summary>
    /// Base class used to define mapping strategies between source and target types.
    /// </summary>
    public abstract class MappingStrategy<TSource, TTarget> : MappingStrategy,
        IMappingStrategy<TSource, TTarget>
        where TSource : class
        where TTarget : class
    {
        public MappingStrategy()
            : base(typeof(TSource), typeof(TTarget)) { }

        // Provides access to the object-mapper for use by derived mapping strategy instances.
        protected IObjectMapper Mapper { get; private set; }

        public override object Map(IObjectMapper mapper, object obj)
        {
            Mapper = mapper;

            // Determine the direction the strategy should be invoked.
            if (SourceType == obj.GetType())
            {
                return SourceToTarget((TSource)obj);
            }
            return TargetToSource((TTarget)obj);
        }

        /// <summary>
        /// Overridden by a derived mapping strategy to map source to target object.
        /// </summary>
        /// <param name="source">The source object to be mapped.</param>
        /// <returns>Instance of the target object.</returns>
        protected virtual TTarget SourceToTarget(TSource source) => default(TTarget);

        /// <summary>
        /// Overridden by a derived mapping strategy to map target to source object.
        /// </summary>
        /// <param name="target">The target object to be mapped.</param>
        /// <returns>Instance of the source object.</returns>
        protected virtual TSource TargetToSource(TTarget target) => default(TSource);
    }
}
