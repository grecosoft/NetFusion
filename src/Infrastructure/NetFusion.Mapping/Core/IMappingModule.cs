using NetFusion.Bootstrap.Plugins;
using System;
using System.Linq;

namespace NetFusion.Mapping.Core
{
    /// <summary>
    /// Interface implemented by a plug-in module responsible for finding all mapping strategies.
    /// </summary>
    public interface IMappingModule : IPluginModuleService
    {
        /// <summary>
        /// A lookup keyed by the source type listing all the possible target mappings.
        /// </summary>
        ILookup<Type, TargetMap> SourceTypeMappings { get; }
    }
}
