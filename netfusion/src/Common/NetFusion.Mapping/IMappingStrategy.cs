using System;

// ReSharper disable UnusedTypeParameter
namespace NetFusion.Mapping
{
    /// <summary>
    /// Interface defining the contract for a type responsible for mapping an object 
    /// between its source to target types.
    /// </summary>
    public interface IMappingStrategy 
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
        /// <param name="obj">The source object to be mapped.</param>
        /// <returns>Resulting instance mapped to the target type.</returns>
        object Map(IObjectMapper mapper, object obj);
    }

    /// <summary>
    /// Interface defining the contract for a type responsible for mapping an object 
    /// between its source to target types.
    /// </summary>
    /// <typeparam name="TSource">The source type associated with the mapping.</typeparam>
    /// <typeparam name="TTarget">The target type to which the source type will be mapped.</typeparam>
    public interface IMappingStrategy<TSource, TTarget> : IMappingStrategy
        where TSource : class
        where TTarget : class
    {

    }
}
