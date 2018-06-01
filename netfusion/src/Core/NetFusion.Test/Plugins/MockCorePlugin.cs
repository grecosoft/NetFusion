using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Test.Plugins
{
    /// <summary>
    /// Mock core plug-in that can be used for testing.  In an actual application
    /// container, core plug-ins implement cross-cutting concerns and implement
    /// specific technical details used to support the application domain.
    /// </summary>
    public class MockCorePlugin : MockPlugin,
        ICorePluginManifest
    {
    }
}
