using System;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;
using NetFusion.Bootstrap.Refactors;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Interface for an implementation responsible for resolving types allowing
    /// the AppContainer to be independent of runtime information such as assemblies.  
    /// </summary>
    public interface ITypeResolver
    {
        void SetPluginMeta(IPluginDefinition plugin);
        void ComposePlugin(IPluginDefinition plugin, IEnumerable<Type> fromPluginTypes);
    }
}