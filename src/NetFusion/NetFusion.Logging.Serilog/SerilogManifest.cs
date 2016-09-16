using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Logging.Serilog
{
    public class SerilogManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public const string ContextPropName = "ContextClrType";

        public string PluginId => "578e90d997ff280dace9cc83";
        public string Name => "Serilog Plug-in";

        public string Description =>
            "Contains an implementation of IContainerLogger that delegates to Serilog.";
    }
}
