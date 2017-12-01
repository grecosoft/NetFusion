using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace NetFusion.Bootstrap.Configuration
{
    /// <summary>
    /// Extension methods for MS Configuration Extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        private const string APP_SETTINGS_FILE_NAME = "appsettings";

        /// <summary>
        /// Configures an ordered list of application JSON files to be searched for settings.
        /// File are search in following order:  
        ///     appsettings.MachineName.json
        ///     appsettings.EnvironmentName.json
        ///     appsettings.json
        /// </summary>
        /// <param name="configBuilder">The configuration to add setting files.</param>
        /// <returns>Instance to the configuration builder.</returns>
        public static ConfigurationBuilder AddDefaultAppSettings(this ConfigurationBuilder configBuilder)
        {
            if (configBuilder == null)
            {
                throw new ArgumentNullException(nameof(configBuilder), "Configuration builder not specified.");
            }

            configBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.json", optional: true, reloadOnChange: true);
            configBuilder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.{EnvironmentConfig.EnvironmentName}.json", reloadOnChange: true, optional: true);
            configBuilder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.{Environment.MachineName}.json", reloadOnChange: true, optional: true);
            return configBuilder;
        }
    }
}
