using System.Collections.Generic;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Services.Mapping;

/// <summary>
/// Interface implemented to provide a list of mapping strategy instances.  The returned strategy instances are 
/// cached and used for all corresponding mappings.  These factories are found and called during the application 
/// bootstrap.  
/// 
/// Note:  often the consuming application will provide one or more such factories returning instances of the 
/// DelegateMap strategy type.  The DelegateMap strategy type is a simple strategy providing a function 
/// delegate specifying how to map one type to another.  The provided function can be from an open-source 
/// mapping library of choice.
/// </summary>
public interface IMappingStrategyFactory : IPluginKnownType
{
    /// <summary>
    /// List of mapping strategy instances.
    /// </summary>
    /// <returns>List of strategies.</returns>
    IEnumerable<IMappingStrategy> GetStrategies();
}