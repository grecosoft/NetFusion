using System;

namespace NetFusion.Mapping
{
    /// <summary>
    /// Defines the contract for a component that determines how a source type is
    /// mapped to its corresponding target type.
    /// </summary>
    public interface IObjectMapper
    {
        /// <summary>
        /// Maps a source type to the specified target type.
        /// </summary>
        /// <typeparam name="TTarget">The type to which the source object should be mapped.  
        /// This can also be a base of the target type associated with the mapping strategy.
        /// When a base type is used, a source object can be mapped to a derived target type
        /// based on the source type.</typeparam>
        /// <param name="source">The source object to be mapped.</param>
        /// <returns>Instance of the target type or a derived instance.</returns>
        TTarget Map<TTarget>(object source) where TTarget : class, new();

        /// <summary>
        ///  Maps a source type to the specified target type.
        /// </summary>
        /// <param name="source">The source object to be mapped.</param>
        /// <param name="targetType">The type to which the source object should be mapped.  
        /// This can also be a base of the target type associated with the mapping strategy.
        /// When a base type is used, a source object can be mapped to a derived target type
        /// based on the source type.</param>
        /// <returns>Instance of the target type or a derived instance.</returns>
        object Map(object source, Type targetType);
    }
}
