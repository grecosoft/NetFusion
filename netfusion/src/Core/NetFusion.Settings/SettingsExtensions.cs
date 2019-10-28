using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Extensions.Reflection;
using System;

namespace NetFusion.Settings
{
    /// <summary>
    /// Extensions for determining a setting class's section path and loading IAppSettings
    /// derived instances.
    /// </summary>
    public static class SettingsExtensions
    {
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
        /// Provided a IAppSettings derived type, determines the section path.
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
        /// Returns an instance of the specified setting type populated from the configuration
        /// section determined by the ConfigurationSection attributes specified on the setting
        /// type.
        /// </summary>
        /// <typeparam name="T">The derived IAppSettings type.</typeparam>
        /// <param name="configuration">The application's configuration.</param>
        /// <param name="defaultValue">the default settings to return if not configured.</param>
        /// <returns>The application settings populated from the configuration.</returns>
        public static T GetSettings<T>(this IConfiguration configuration, T defaultValue = null)
            where T : class, IAppSettings
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            string sectionPath = GetSectionPath<T>();
            if (sectionPath == null)
            {
                throw new InvalidOperationException(
                    $"The section path for type: {typeof(T)} could not be determined.");
            }

            T settings = configuration.GetSection(sectionPath).Get<T>() ?? defaultValue;
            if (settings == null)
            {
                throw new InvalidOperationException(
                    $"Settings could not be loaded from configuration section path: {sectionPath} " + 
                    $"for the settings of type: {typeof(T)}.");
            }

            ValidateSettings(settings);
            return settings;
        }

        private static void ValidateSettings(IAppSettings settings)
        {
            IObjectValidator validator = new DataAnnotationsValidator(settings);
            var result = validator.Validate();

            if (result.IsInvalid)
            {
                string section = GetSectionPath(settings.GetType());
                throw new SettingsValidationException(settings.GetType(), section, result);
            }
        }

        internal static void ValidateSettings(ILogger logger, IAppSettings settings)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            
            IObjectValidator validator = new DataAnnotationsValidator(settings);
            var result = validator.Validate();

            if (result.IsInvalid)
            {
                string section = GetSectionPath(settings.GetType());

                logger.LogErrorDetails(
                    $"Invalid application setting: {settings.GetType().FullName}. " + 
                    $"With configuration section: {section}.", result.ObjectValidations);
            }

            result.ThrowIfInvalid();
        }
    }
}
