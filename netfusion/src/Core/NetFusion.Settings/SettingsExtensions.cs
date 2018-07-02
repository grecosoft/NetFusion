using Microsoft.Extensions.Configuration;
using NetFusion.Common.Extensions.Reflection;
using System;

namespace NetFusion.Settings
{
    /// <summary>
    /// Extensions for determining a setting classes's section path based on the 
    /// found ConfigurationSection attributes specified on the class and its parents.
    /// The concatenation of each name specified by the ConfigurationSetion attribute
    /// determines the settings classes's complete section path.
    /// </summary>
    public static class SettingsExtensions
    {
        /// <summary>
        /// Provided a IAppSetting derived instance,  determines the section path by recursively 
        /// walking the type hierarchy and finding all ConfigurationSection attributes and 
        /// concatenating the section name associated with each attribute.
        /// </summary>
        /// <param name="appSettings">The application setting to determine section path.</param>
        /// <returns>The section path associated with the settings type.  If not found,
        /// null value will be returned.</returns>
        public static string GetSectionPath(this IAppSettings appSettings)
        {
            if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));

            return GetSectionPath(appSettings.GetType());
        }

        /// <summary>
        /// Provided a IAppSettings derived type, determines the section path by recursively 
        /// walking the type hierarchy and finding all ConfigurationSection attributes and 
        /// concatenating the section name associated with each attribute.
        /// </summary>
        /// <typeparam name="T">The application setting type to determine section path.</typeparam>
        /// <returns>The section path associated with the settings type.  If not found,
        /// null value will be returned.</returns>
        public static string GetSectionPath<T>()
           where T : IAppSettings
        {
            var settingsType = typeof(T);
            return GetSectionPath(settingsType);
        }

        /// <summary>
        /// Returns the section path for a given settings type.
        /// </summary>
        /// <param name="settingsType">Setting runtime type.</param>
        /// <returns>The section path associated with the settings type.  If not found,
        /// null value will be returned.</returns>
        public static string GetSectionPath(Type settingsType)
        {
            return settingsType.GetAttribute<ConfigurationSectionAttribute>()?.SectionName;
        }

        /// <summary>
        /// Returns an instance of the specified setting type populated from the configuration
        /// section determined by the ConfigurationSection attributes specified on the setting
        /// type.
        /// </summary>
        /// <typeparam name="T">The derived IAppSettings type.</typeparam>
        /// <param name="configuration">The application's configuration.</param>
        /// <returns>The application settings populated from the configuration.</returns>
        public static T GetSettings<T>(this IConfiguration configuration)
            where T : class, IAppSettings
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string sectionPath = GetSectionPath<T>();
            if (sectionPath == null)
            {
                throw new InvalidOperationException(
                    $"The section path for type: {typeof(T)} could not be determined.");
            }

            return configuration.GetSection(sectionPath).Get<T>();
        }
    }
}
