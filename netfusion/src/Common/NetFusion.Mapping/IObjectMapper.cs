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
        /// Maps a source type to the specified target type or derived type.
        /// </summary>
        /// <typeparam name="TTarget">The type to which the source object should be mapped.  
        /// This can also be a base of the target type associated with the mapping strategy.
        /// When a base type is used, a source object can be mapped to a derived target type.
        /// </typeparam>
        /// <param name="source">The source object to be mapped.</param>
        /// <returns>Instance of the target type or a derived instance.  If the source
        /// type could not be mapped to a target type, an exception is raised.</returns>
        TTarget Map<TTarget>(object source) where TTarget : class;

        /// <summary>
        /// Maps a source type to the specified target type or derived type.
        /// </summary>
        /// <param name="source">The source object to be mapped.</param>
        /// <param name="targetType">The type to which the source object should be mapped.  
        /// This can also be a base of the target type associated with the mapping strategy.
        /// When a base type is used, a source object can be mapped to a derived target type.
        /// </param>
        /// <returns>Instance of the target type or a derived instance.  If the source type
        /// could not be mapped to a target type, an exception is raised.</returns>
        object Map(object source, Type targetType);

        /// <summary>
        /// Maps a source type to the specified target type or derived type.  If the source
        /// object could not be mapped, false is returned.  Otherwise true is returned and
        /// the mapped result returned as an output parameter.
        /// </summary>
        /// <param name="source">The source object to be mapped.</param>
        /// <param name="targetType">The type to which the source object should be mapped.  
        /// This can also be a base of the target type associated with the mapping strategy.
        /// When a base type is used, a source object can be mapped to a derived target type.
        /// </param>
        /// <param name="mappedResult">The result if the source object could be mapped.</param>
        /// <returns>True if the source object could be mapped.  Otherwise False is returned.</returns>
        bool TryMap(object source, Type targetType, out object mappedResult);

        /// <summary>
        /// Maps a source type to the specified target type or derived type.  If the source
        /// object could not be mapped, false is returned.  Otherwise true is returned and
        /// the mapped result returned as an output parameter.
        /// </summary>
        /// <param name="source">The source object to be mapped.</param>
        /// <typeparam name="TTarget">The type to which the source object should be mapped.  
        /// This can also be a base of the target type associated with the mapping strategy.
        /// When a base type is used, a source object can be mapped to a derived target type.
        /// </typeparam>
        /// <param name="mappedResult">The result if the source object could be mapped.</param>
        /// <returns>True if the source object could be mapped.  Otherwise False is returned.</returns>
        bool TryMap<TTarget>(object source, out TTarget mappedResult) where TTarget : class;
    }
}
