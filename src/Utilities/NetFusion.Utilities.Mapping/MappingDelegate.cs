using NetFusion.Common;
using NetFusion.Utilities.Core;
using System;

namespace NetFusion.Utilities.Mapping
{
    /// <summary>
    /// Mapping strategy for which the created instance delegates to specified
    /// mapping function. 
    /// </summary>
    /// <typeparam name="TSource">The source type of the object being mapped.</typeparam>
    /// <typeparam name="TTarget">The target type of the object being mapped.</typeparam>
    public sealed class MappingDelegate<TSource, TTarget> : MappingStrategy
    {
        private Func<TSource, TTarget> _map;

        /// <summary>
        /// Constructor used to create mapping strategy delegating to a specified
        /// mapping function.
        /// </summary>
        /// <param name="map">Function mapping source object to target object instance.</param>
        public MappingDelegate(Func<TSource, TTarget> map)
            : base(typeof(TSource), typeof(TTarget))
        {
            Check.NotNull(map, nameof(map), "mapping function not specified.");
            _map = map;
        }

        public override object Map(IObjectMapper mapper, IAutoMapper autoMapper, object obj)
        {
            return _map((TSource)obj);
        }
    }
}
