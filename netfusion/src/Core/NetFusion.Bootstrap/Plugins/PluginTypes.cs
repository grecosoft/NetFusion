namespace NetFusion.Bootstrap.Plugins
{
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
        /// application's implementation.  For example, it is common to have the following:
        /// 
        /// [AppContext.Domain]    - The domain models, commands, queries, and domain-events.
        ///                        - This plugin assembly should contain no dependencies on other plugins.
        /// 
        /// [AppContext.App]       - Contains command, query, and domain-event handlers.  Also contains service
        ///                          implementations. This plugin assembly should only reference the assembly of
        ///                          of the domain plugin.
        ///
        /// [AppContext.Infra]     - Contains repository implementations.  Also contains any implementation
        ///                          specific components encapsulating dependencies on external services.
        ///                        - This plugin assembly should only reference the assembly of the domain
        ///                          plugin.
        /// </summary>
        ApplicationPlugin = 2,
        
        /// <summary>
        /// Core plugins contain reusable and crosscutting implementations that can optionally
        /// be used by applications.
        /// </summary>
        CorePlugin = 3
    }
}