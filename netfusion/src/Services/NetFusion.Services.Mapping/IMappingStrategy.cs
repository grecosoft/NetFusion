using System;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Services.Mapping;

/// <summary>
/// Interface defining the contract for a type responsible for mapping an object 
/// between its source to target types.
/// </summary>
public interface IMappingStrategy : IPluginKnownType
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