namespace Service.Client
{
    using NetFusion.Bootstrap.Manifests;

    public class HostManifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "AD13D4BB-8777-4F73-8C6E-23BD03ABC433";
        public string Name => "Example Client Host";
        public string Description => "Test Client host application.";
    }
}