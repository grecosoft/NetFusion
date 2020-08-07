using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NetFusion.Builder
{
    /// <summary>
    /// Extension methods for MS Configuration Extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        private const string AppSettingsFileName = "appsettings";

        /// <summary>
        /// Configures an ordered list of application JSON files to be searched for settings.
        /// Files are search in following order.  When a value of a variable is found in multiple
        /// sources, the value of the last most listed source is used.
        ///   appsettings.json
        ///   appsettings.EnvironmentName.json
        ///   appsettings.MachineName.json
        /// </summary>
        /// <param name="builder">The configuration to add setting providers.</param>
        /// <param name="hostingEnv">The environment of the host.</param>
        /// <returns>Instance to the configuration builder.</returns>
        public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder builder,
            IHostEnvironment hostingEnv)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "Configuration builder not specified.");
        
            builder.SetBasePath(Directory.GetCurrentDirectory()); 
            builder.AddJsonFile($"{AppSettingsFileName}.json", reloadOnChange: true, optional: true);
            builder.AddJsonFile($"{AppSettingsFileName}.{hostingEnv.EnvironmentName}.json", reloadOnChange: true, optional: true);
            builder.AddJsonFile($"{AppSettingsFileName}.{Environment.MachineName}.json", reloadOnChange: true, optional: true);
            return builder;
        }

        /// <summary>
        /// Configures settings for the preferred defaults when developing applications running in a Docker container.
        /// When executing within a developer environment, the applications settings are first sourced from application
        /// setting files by calling AddAppSettings which are then overridden by any matching environment variables.
        /// When the application is not executing within the development environment, all variables are sourced from
        /// environment variables.
        /// </summary>
        /// <param name="builder">The configuration to add setting providers.</param>
        /// <param name="hostingEnv">The environment of the host.</param>
        /// <returns>Instance of the configuration builder.</returns>
        public static IConfigurationBuilder AddDockerDefaultSettings(this IConfigurationBuilder builder,
            IHostEnvironment hostingEnv)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "Configuration builder not specified.");
            
            if (hostingEnv.IsDevelopment())
            {
                builder.AddAppSettings(hostingEnv);    
            }

            builder.AddEnvironmentVariables();
            return builder;
        }
    }
}