using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Test.Plugins
{
    /// <summary>
    /// The application plug-in represents the executing host application
    /// such as a WebApi or Console host.  A container can only have one
    /// associated application plug-in.
    /// </summary>
    public class MockAppHostPlugin : MockPlugin,
        IAppHostPluginManifest
    {

    }
}
