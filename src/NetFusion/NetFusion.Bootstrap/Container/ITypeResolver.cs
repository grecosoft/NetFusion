using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Interface for an implementation responsible for resolving types allowing
    /// the AppContainer to be independent of runtime information such as assemblies.  
    /// </summary>
    public interface ITypeResolver
    {
        /// <summary>
        /// The optional location where the type resolver will probe for assemblies
        /// when discovering plug-ins.
        /// </summary>
        string[] SearchPatterns { get; }

        /// <summary>
        /// Locates all assemblies containing plug-in manifests and populates
        /// the registry with manifest instances.
        /// </summary>
        /// <param name="registry">The registry to have its manifest property populated.</param>
        void DiscoverManifests(ManifestRegistry registry);

        /// <summary>
        /// Loads all types found within the corresponding plug-in.
        /// </summary>
        /// <param name="plugin">The plug-in to load.</param>
        void LoadPluginTypes(Plugin plugin);

        /// <summary>
        /// Populates the plug-in's modules from its contained types
        /// that implement the IPluginModule interface.
        /// </summary>
        /// <param name="plugin"></param>
        void DiscoverModules(Plugin plugin);

        /// <summary>
        /// Populates all properties on the specified module that are an enumeration of
        /// IPluginKnowType with instances of the corresponding types found in the list 
        /// of provided plug-in types.
        /// </summary>
        /// <param name="forModule">The module to have known type properties populated.</param>
        /// <param name="fromPluginTypes">The list of types from which instances should be created.</param>
        /// <returns>The know types defined by the module.</returns>
        IEnumerable<Type> DiscoverKnownTypes(IPluginModule forModule, IEnumerable<PluginType> fromPluginTypes);
    }
}
