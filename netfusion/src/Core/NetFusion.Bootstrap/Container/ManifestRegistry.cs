using NetFusion.Bootstrap.Manifests;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Class containing all the discovered manifests used to identify plug-in
    /// assemblies from which plug-ins are created and bootstrapped.
    /// </summary>
    public class ManifestRegistry
    {
        /// <summary>
        /// All manifest instances used to identify plug-ins.
        /// </summary>
        public List<IPluginManifest> AllManifests { get; set; }

        /// <summary>
        /// The manifest that identifies the host application.
        /// </summary>
        /// <returns>Application specific manifest information.</returns>
        public IAppHostPluginManifest AppHostPluginManifest => 
            AppHostPluginManifests.FirstOrDefault();

        /// <summary>
        /// The manifest that identifies the host application.  This collection must 
        /// have one item to be valid. There can only be one application host manifest.
        /// </summary>
        /// <returns>Application specific manifest information.</returns>
        public IEnumerable<IAppHostPluginManifest> AppHostPluginManifests => 
            AllManifests.OfType<IAppHostPluginManifest>();

        /// <summary>
        /// The manifests that identify the plug-ins that are specific to the application.
        /// </summary>
        /// <returns>
        /// Application components specific manifest information.
        /// </returns>
        public IEnumerable<IAppComponentPluginManifest> AppComponentPluginManifests => 
            AllManifests.OfType<IAppComponentPluginManifest>();

        /// <summary>
        /// The manifests that identifying core plug-ins.
        /// </summary>
        /// <returns>Plug-in component specific manifest information.</returns>
        public IEnumerable<ICorePluginManifest> CorePluginManifests => 
            AllManifests.OfType<ICorePluginManifest>();
    }
}
