using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Settings
{
    /// <summary>
    /// Interface representing a class containing application settings.  An instance of 
    /// the implementing class is created and initialized when injected into a dependent 
    /// component.  Once, initialized, the same application settings object is returned 
    /// for the life of the application.
    /// </summary>
    public interface IAppSettings : IKnownPluginType
    {
        /// <summary>
        /// The identity value of the application setting.
        /// </summary>
        string AppSettingsId { get; set; }

        /// <summary>
        /// The identity value of the host application that is 
        /// specified in the host plug-in manifest.
        /// </summary>
        string ApplicationId {get; set; }

        /// <summary>
        /// The environment for which the settings should be loaded.
        /// </summary>
        EnvironmentTypes Environment { get; set; }

        /// <summary>
        /// The machine associated with the configuration.
        /// </summary>
        string MachineName { get; set; }

        /// <summary>
        /// Determines if a default uninitialized instance of the settings class
        /// can be used if no corresponding initialization strategy is found.
        /// </summary>
        bool IsInitializationRequired { get; set; }
    }
}
