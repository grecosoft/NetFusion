using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetFusion.Bootstrap.Configuration
{
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Extension methods for MS Configuration Extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        private const string AppSettingsFileName = "appsettings";

        /// <summary>
        /// Configures an ordered list of application JSON files to be searched for settings.
        /// Files are search in following order:  
        ///     appsettings.MachineName.json
        ///     appsettings.EnvironmentName.json
        ///     appsettings.json
        /// </summary>
        /// <param name="builder">The configuration to add setting providers.</param>
        /// <param name="environmentName"></param>
        /// <returns>Instance to the configuration builder.</returns>
        public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder builder,
            string environmentName)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "Configuration builder not specified.");
        
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile($"{AppSettingsFileName}.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile($"{AppSettingsFileName}.{environmentName}.json", reloadOnChange: true, optional: true);
            builder.AddJsonFile($"{AppSettingsFileName}.{Environment.MachineName}.json", reloadOnChange: true, optional: true);
            return builder;
        }

        /// <summary>
        /// Configures settings for the preferred defaults when developing applications running in a Docker container.
        /// When executing within a developer environment, the applications settings are first sourced from environment
        /// variables and then by the sources configured by the AddAppSettings method.  When the application
        /// is not executing within the development environment, all variables are sourced from environment variables.
        /// </summary>
        /// <param name="builder">The configuration to add setting providers.</param>
        /// <param name="environmentName"></param>
        /// <returns>Instance of the configuration builder.</returns>
        public static IConfigurationBuilder AddDockerDefaultSettings(this IConfigurationBuilder builder,
            string environmentName)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "Configuration builder not specified.");
            
            if (environmentName == EnvironmentName.Development)
            {
                builder.AddAppSettings(environmentName);    
            }

            builder.AddEnvironmentVariables();
            return builder;
        }

        /// <summary>
        /// Should be called after adding all settings sources to override any existing configured settings with a set
        /// of in memory values.
        /// </summary>
        /// <param name="builder">The configuration to override with set of values.</param>
        /// <param name="values">The values to add to the configuration.</param>
        /// <returns>Instance of the configuration builder.</returns>
        public static IConfigurationBuilder AddInMemoryOverrides(this IConfigurationBuilder builder,
            IDictionary<string, string> values)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "Configuration builder not specified.");
            if (values == null) throw new ArgumentNullException(nameof(values), "Settings not specified.");
            
            return builder.AddInMemoryCollection(values);
        }
    }
}