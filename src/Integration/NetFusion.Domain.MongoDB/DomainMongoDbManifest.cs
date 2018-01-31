using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Domain.MongoDB
{
    public class DomainMongoDbManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "F05B7917-504F-4753-BE02-DD9FD5D53B7D";
        public string Name => "NetFusion Domain MongoDB Plug-in";

        public string Description =>
            "Plug-in that provides domain entity MongoDB based implementations.";
    }
}
