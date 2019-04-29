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
        
        
        
        
        
        
        /// <summary>
        /// Called by the AppContainer on the passed ITypeResolver instance.
        /// </summary>
        /// <param name="loggerFactory">Logger factory that can be used for logging.</param>
        void Initialize(ILoggerFactory loggerFactory);
       
        /// <summary>
        /// Locates all assemblies containing plug-in manifests and populates the 
        /// registry with manifest instances.
        /// </summary>
        /// <param name="registry">The registry to have its manifest property populated.</param>
        void SetPluginManifests(ManifestRegistry registry);

        /// <summary>
        /// Loads all types based on a set of conventions from which a plug-in is built.
        /// </summary>
        /// <param name="plugin">The plug-in to load.</param>
        void SetPluginResolvedTypes(Plugin plugin);

        /// <summary>
        /// Populates all properties on the specified module that are an enumeration of
        /// IPluginKnowType with instances of the corresponding types found in the list 
        /// of provided plug-in types.
        /// </summary>
        /// <param name="forModule">The module to have known type properties populated.</param>
        /// <param name="fromPluginTypes">The list of types from which instances should be created.</param>
        void SetPluginModuleKnownTypes(IPluginModule forModule, IEnumerable<PluginType> fromPluginTypes);
    }
}