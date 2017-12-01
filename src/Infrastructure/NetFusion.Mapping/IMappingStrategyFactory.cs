using NetFusion.Base.Plugins;
using System.Collections.Generic;

namespace NetFusion.Mapping
{
    /// <summary>
    /// Interface implemented to provide a list of mapping strategy instances.  The returned strategy instances are 
    /// cached and used for all corresponding mappings.  These factories are found and called during the application 
    /// bootstrap.  
    /// 
    /// Note:  often the consuming application will provide one or more such factories returning instances of the 
    /// MappingDelegate strategy type.  The MappingDelegate strategy type is a simple strategy providing a function 
    /// delegate specifying how to map one type to another.  The provided function is often from an open-source 
    /// mapping library of choice.
    /// </summary>
    public interface IMappingStrategyFactory : IKnownPluginType
    {
        /// <summary>
        /// List of mapping strategy instances.
        /// </summary>
        /// <returns>List of strategies.</returns>
        IEnumerable<IMappingStrategy> GetStrategies();
    }
}
