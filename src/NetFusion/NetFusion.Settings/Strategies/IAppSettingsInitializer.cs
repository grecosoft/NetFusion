using NetFusion.Bootstrap.Plugins;
using System;

namespace NetFusion.Settings.Strategies
{
    /// <summary>
    /// Base interface inherited from the derived generic version.
    /// Implemented by classes that are responsible for initializing
    /// application specific setting classes.</summary>
    public interface IAppSettingsInitializer : IKnownPluginType
    {
        /// <summary>
        /// The type of the setting loaded by the initializer.
        /// </summary>
        Type SettingsType { get; }

        /// <summary>
        /// Called to configure the settings.
        /// </summary>
        /// <param name="settings">The settings to initialize.</param>
        /// <returns>The passed settings instance initialized, a new 
        /// initialized instance of the class, or null if the initializer
        /// cannot load the settings.</returns>
        IAppSettings Configure(IAppSettings settings);
    }

    /// <summary>
    /// Implemented by classes that are responsible for initializing
    /// application specific setting classes.
    /// </summary>
    /// <typeparam name="TSettings">The application settings type associated
    /// with the initializer.</typeparam>
    public interface IAppSettingsInitializer<in TSettings> : IAppSettingsInitializer
        where TSettings : IAppSettings
    {

    }
}
