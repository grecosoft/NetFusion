using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Extensions.Reflection;
using System;

namespace NetFusion.Settings
{
    /// <summary>
    /// Extensions for determining a setting class's section path based on the 
    /// found ConfigurationSection attributes specified on the class and its parents.
    /// The concatenation of each name specified by the ConfigurationSetion attribute
    /// determines the settings class's complete section path.
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
        /// <param name="logger">The logger to write configuration validation errors.</param>
        /// <returns>The application settings populated from the configuration.</returns>
        public static T GetSettings<T>(this IConfiguration configuration, ILogger logger)
            where T : class, IAppSettings
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string sectionPath = GetSectionPath<T>();
            if (sectionPath == null)
            {
                throw new InvalidOperationException(
                    $"The section path for type: {typeof(T)} could not be determined.");
            }

            T settings = configuration.GetSection(sectionPath).Get<T>();
            if (settings == null)
            {
                throw new InvalidOperationException(
                    $"Settings could not be loaded from configuration section path: {sectionPath} " + 
                    $"for the settings of type: {typeof(T)}.");
            }

            ValidateSettings(logger, settings);
            return settings;
        }

        internal static void ValidateSettings(ILogger logger, object settings)
        {
            var validator = new DataAnnotationsValidator(settings);
            var result = validator.Validate();

            if (result.IsInvalid)
            {
                string section = SettingsExtensions.GetSectionPath(settings.GetType());

                logger.LogErrorDetails(
                    $"Invalid application setting: {settings.GetType().FullName}. " + 
                    $"With configuration section: {section}.", result.ObjectValidations);
            }

            result.ThrowIfInvalid();
        }
    }
}
