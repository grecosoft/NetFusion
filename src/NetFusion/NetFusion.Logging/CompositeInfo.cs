using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Logging
{
    /// <summary>
    /// Child model of the HostLog model that is returned to a client responsible
    /// for rendering the host log.
    /// </summary>
    public class CompositeInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="plugins">List of plug-in models for each plug-in contained 
        /// within the compose application.</param>
        public CompositeInfo(IEnumerable<PluginInfo> plugins)
        {
            this.AppHostPlugin = plugins.First(pi => pi.Plugin.PluginType == PluginTypes.AppHostPlugin);
            this.AppComponentPlugins = plugins.Where(pi => pi.Plugin.PluginType == PluginTypes.AppComponentPlugin);
            this.CorePlugins = plugins.Where(pi => pi.Plugin.PluginType == PluginTypes.CorePlugin);
        }

        public PluginInfo AppHostPlugin { get; }
        public IEnumerable<PluginInfo> AppComponentPlugins { get; }
        public IEnumerable<PluginInfo> CorePlugins { get; }
    }
}
