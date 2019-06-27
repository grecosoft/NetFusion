using System;
using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Interface for an implementation responsible for resolving types allowing
    /// the composite-application to be independent of runtime information such
    /// as assemblies.  
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// When invoked by the CompositeContainer, the implementation should invoke
        /// the SetPluginMeta on the passed plugin.
        /// </summary>
        /// <param name="plugin">The plugin to have its metadata set.</param>
        void SetPluginMeta(IPlugin plugin);
        
        /// <summary>
        /// When invoked by the CompositeContainer, the implementation should check each
        /// plugin module for IEnumerable properties where the generic parameter derives
        /// from IKnownPluginType.  Each of these properties should be set to an array
        /// containing all derived concrete instances.
        /// </summary>
        /// <param name="plugin">The plugin to be composed.</param>
        /// <param name="fromPluginTypes">The list of types from which the array
        /// of IKnownPluginType derived instances are created.</param>
        void ComposePlugin(IPlugin plugin, IEnumerable<Type> fromPluginTypes);
    }
}