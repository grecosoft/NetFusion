namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Identifies the category of plug-in.
    /// </summary>
    public enum PluginTypes
    {
        /// <summary>
        /// The application host.  Most often a WebApi or console executable.
        /// </summary>
        AppHostPlugin,

        /// <summary>
        /// Plug-ins containing application centric components such as Domain Entities,
        /// services, and repositories.
        /// </summary>
        AppComponentPlugin,

        /// <summary>
        /// Plug-ins containing cross-cutting concerns.
        /// </summary>
        CorePlugin
    }
}
