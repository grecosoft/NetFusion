namespace NetFusion.Test.Plugins
{
    using NetFusion.Bootstrap.Plugins;

    /// <summary>
    /// Mock application component plug-in that can be used for testing.
    /// The application container can have multiple associated application
    /// components.  This components in this type of plug-in are specific 
    /// to the domain of the application.  Domain Entities, Aggregates, 
    /// Services, and Repositories are such examples.
    /// </summary>
    public class MockApplicationPlugin : MockPlugin
    {
        public MockApplicationPlugin() : base(PluginTypes.ApplicationPlugin)
        {
            
        }
    }
}
