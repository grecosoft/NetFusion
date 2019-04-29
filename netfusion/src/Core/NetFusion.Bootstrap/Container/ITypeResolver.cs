using System;
using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Interface for an implementation responsible for resolving types allowing
    /// the AppContainer to be independent of runtime information such as assemblies.  
    /// </summary>
    public interface ITypeResolver
    {
        void SetPluginMeta(IPlugin plugin);
        void ComposePlugin(IPlugin plugin, IEnumerable<Type> fromPluginTypes);
    }
}