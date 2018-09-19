using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Redis
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "6A52A70C-719B-41CF-AEFC-7CDFB586627A";
        public string Name => "Redis Plugin.";
        public string Description => "Plugin containing Redis extensions and Pub/Sub messaging.";
    }
}
