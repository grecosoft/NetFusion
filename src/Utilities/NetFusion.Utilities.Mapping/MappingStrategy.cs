using NetFusion.Common.Extensions.Reflection;
using NetFusion.Utilities.Core;
using System;

namespace NetFusion.Utilities.Mapping
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
            this.SourceType = sourceType;
            this.TargetType = targetType;
        }

        public abstract object Map(IAutoMapper autoMapper, object obj);
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


        public override object Map(IAutoMapper autoMapper, object obj)
        {
            // Determine the direction the strategy should be invoked.
            if (SourceType == obj.GetType())
            {
                return MapSource(autoMapper, obj);
            }
            return MapTarget(autoMapper, obj);
        }

        private object MapSource(IAutoMapper autoMapper, object source)
        {
            TTarget target = SourceToTarget((TSource)source);
            if (target != null)
            {
                return target;
            }

            // Create and populate target type and apply additional derived mappings.
            if (this.TargetType.HasDefaultConstructor())
            {
                target = autoMapper.Map<TTarget>(source);
                SourceToTarget((TSource)source, target);
            }
           
            return target;
        }

        private object MapTarget(IAutoMapper autoMapper, object target)
        {
            TSource source = TargetToSource((TTarget)target);
            if (source != null)
            {
                return source;
            }

            // Create and populate source type and apply additional derived mappings.
            if (this.SourceType.HasDefaultConstructor())
            {
                source = autoMapper.Map<TSource>(target);
                TargetToSource((TTarget)target, source);
            }

            return source;
        }

        /// <summary>
        /// Overridden by a derived mapping strategy to map source to target object.
        /// </summary>
        /// <param name="source">The source object to be mapped.</param>
        /// <returns>Instance of the target object.</returns>
        protected virtual TTarget SourceToTarget(TSource source) => default(TTarget);

        /// <summary>
        /// Overridden by a derived mapping strategy to apply additional mappings to the
        /// provided populated target entity.
        /// </summary>
        /// <param name="source">The source object to be mapped.</param>
        /// <param name="target">Instance to an auto populated target object.</param>
        protected virtual void SourceToTarget(TSource source, TTarget target) { }

        /// <summary>
        /// Overridden by a derived mapping strategy to map target to source object.
        /// </summary>
        /// <param name="target">The target object to be mapped.</param>
        /// <returns>Instance of the source object.</returns>
        protected virtual TSource TargetToSource(TTarget target) => default(TSource);

        /// <summary>
        /// Overridden by a derived mapping strategy to apply additional mappings to the
        /// provided populated source entity.
        /// </summary>
        /// <param name="target">The target object to be mapped.</param>
        /// <param name="source">Instance of an auto populated source object.</param>
        protected virtual void TargetToSource(TTarget target, TSource source) { }
    }
}
