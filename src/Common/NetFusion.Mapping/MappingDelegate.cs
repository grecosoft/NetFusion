using System;

namespace NetFusion.Mapping
{
    /// <summary>
    /// Mapping strategy for which the created instance delegates to specified mapping function. 
    /// The mapping function will usually delegate to an open-source mapping library of choice.
    /// Note: the consuming application returns instances of this delegate strategy type by 
    /// defining one or more IMappingStrategyFactory instances.  This was created since many
    /// of the newer mapping libraries are generic based.
    /// </summary>
    /// <typeparam name="TSource">The source type of the object being mapped.</typeparam>
    /// <typeparam name="TTarget">The target type of the object being mapped.</typeparam>
    public sealed class MappingDelegate<TSource, TTarget> : MappingStrategy
    {
        private Func<TSource, TTarget> _map;

        /// <summary>
        /// Constructor used to create mapping strategy delegating to a specified mapping function.
        /// </summary>
        /// <param name="map">Function mapping source object to target object instance.</param>
        public MappingDelegate(Func<TSource, TTarget> map)
            : base(typeof(TSource), typeof(TTarget))
        {
            _map = map ?? throw new ArgumentNullException(nameof(map), 
                "Mapping function not specified."); ;
        }

        public override object Map(IObjectMapper mapper, object obj)
        {
            return _map((TSource)obj);
        }
    }
}
