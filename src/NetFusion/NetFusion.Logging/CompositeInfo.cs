using NetFusion.Bootstrap.Plugins;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Logging
{
    public class CompositeInfo
    {
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
