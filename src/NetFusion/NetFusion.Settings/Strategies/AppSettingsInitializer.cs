using System;

namespace NetFusion.Settings.Strategies
{
    /// <summary>
    /// Default implementation of the IAppSettingsInitializer interface 
    /// from which application settings initializer classes can derive.
    /// </summary>
    /// <typeparam name="TSettings">The application settings type the 
    /// derived class initializes.</typeparam>
    public abstract class AppSettingsInitializer<TSettings> : IAppSettingsInitializer<TSettings>
        where TSettings : IAppSettings
    {
        public Type SettingsType { get { return typeof(TSettings); } }

        public IAppSettings Configure(IAppSettings settings)
        {
            return OnConfigure((TSettings)settings);
        }

        /// <summary>
        /// Must be overridden by the derived settings initializer class to
        /// initialize the application settings.  If the derived class can't 
        /// initialize the settings, it should return null so the next listed
        /// initializer can be executed.
        /// </summary>
        /// <param name="settings">The application settings to be initialized.</param>
        /// <returns>The passed settings instance initialized, a new 
        /// initialized instance of the class, or null if the initializer
        /// cannot load the settings.</returns>
        protected abstract IAppSettings OnConfigure(TSettings settings);
    }
}

