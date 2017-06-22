using NetFusion.Base.Plugins;
using NetFusion.Utilities.Core;
using System;

namespace NetFusion.Utilities.Mapping
{
    /// <summary>
    /// Interface defining the contract for a type responsible for mapping an object 
    /// from its source to target type.
    /// </summary>
    public interface IMappingStrategy : IKnownPluginType
    {
        /// <summary>
        /// The source type associated with the mapping.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// The target type to which the source type will be mapped.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Maps a source type to its corresponding target type.
        /// </summary>
        /// <param name="mapper">Reference to the IObjectMapper service.</param>
        /// <param name="autoMapper">Reference to the configured IAutoMapper.</param>
        /// <param name="obj">The source object to be mapped.</param>
        /// <returns>The resulting target type instance.</returns>
        object Map(IObjectMapper mapper, IAutoMapper autoMapper, object obj);
    }

    /// <summary>
    /// Interface defining the contract for a type responsible for mapping from
    /// an object from its source to target type.
    /// </summary>
    /// <typeparam name="TSource">The source type associated with the mapping.</typeparam>
    /// <typeparam name="TTarget">The target type to which the source type will be mapped.</typeparam>
    public interface IMappingStrategy<TSource, TTarget> : IMappingStrategy
        where TSource : class
        where TTarget : class
    {

    }
}
