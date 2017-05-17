using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Test.Plugins
{
    /// <summary>
    /// Mock core plug-in that can be used for testing.
    /// </summary>
    public class MockCorePlugin : MockPlugin,
        ICorePluginManifest
    {
    }
}
