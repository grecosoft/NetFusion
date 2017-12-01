using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Test.Plugins
{
    /// <summary>
    /// Mock application component plug-in that can be used for testing.
    /// The application container can have multiple associated application
    /// components.  This components in this type of plug-in are specific 
    /// to the domain of the application.  Domain Entities, Aggregates, 
    /// Services, and Repositories are such examples.
    /// </summary>
    public class MockAppComponentPlugin : MockPlugin,
        IAppComponentPluginManifest
    {

    }
}
