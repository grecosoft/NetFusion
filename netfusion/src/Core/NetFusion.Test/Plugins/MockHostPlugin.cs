namespace NetFusion.Test.Plugins
{
    using NetFusion.Bootstrap.Plugins;

    /// <summary>
    /// The host plug-in represents the executing host application such as 
    /// a WebApi or Console host.  A container can only have one associated
    /// host plug-in.
    /// </summary>
    public class MockHostPlugin : MockPlugin
    {
        public MockHostPlugin() : base(PluginTypes.HostPlugin)
        {
            
        }
    }
}
