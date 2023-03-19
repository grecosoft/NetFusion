namespace NetFusion.Core.Bootstrap.Plugins;

/// <summary>
/// Used to identify a type of plugin.
/// </summary>
public enum PluginTypes
{
    /// <summary>
    /// This is the process executing the application and defining the GenericHost.
    /// There can only be one host plugin added to the CompositeCollection.
    /// </summary>
    HostPlugin = 1,
        
    /// <summary>
    /// An application level centric plugin.  Application centric plugins are used to organize an
    /// application's implementation.
    /// </summary>
    AppPlugin = 2,
        
    /// <summary>
    /// Core plugins contain reusable and crosscutting implementations that can optionally
    /// be used by applications.
    /// </summary>
    CorePlugin = 3
}