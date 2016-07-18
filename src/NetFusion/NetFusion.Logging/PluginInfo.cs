using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Logging
{
    public class PluginInfo
    {
        public PluginInfo(Plugin plugin)
        {
            this.Plugin = plugin;

            this.Name = plugin.Manifest.Name;
            this.PluginId = plugin.Manifest.PluginId;
            this.PluginType = MapPluginType(plugin.PluginType);
        }

        internal Plugin Plugin { get; }

        public string Name { get; }
        public string PluginId { get; }
        public string PluginType { get; }

        private string MapPluginType(PluginTypes pluginType)
        {
            switch (pluginType)
            {
                case PluginTypes.AppHostPlugin:
                    return "Host";

                case PluginTypes.AppComponentPlugin:
                    return "Application";

                case PluginTypes.CorePlugin:
                    return "Core";
            }
            return null;
        }
    }
}
