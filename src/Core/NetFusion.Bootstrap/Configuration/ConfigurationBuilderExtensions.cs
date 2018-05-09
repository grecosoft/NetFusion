﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        /// Files are search in following order:  
        ///     appsettings.MachineName.json
        ///     appsettings.EnvironmentName.json
        ///     appsettings.json
        /// </summary>
        /// <param name="builder">The configuration to add setting providers.</param>
        /// <returns>Instance to the configuration builder.</returns>
        public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "Configuration builder not specified.");
        
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.{EnvironmentConfig.EnvironmentName}.json", reloadOnChange: true, optional: true);
            builder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.{Environment.MachineName}.json", reloadOnChange: true, optional: true);
            return builder;
        }

        /// <summary>
        /// Configures settings for the preferred defaults when developing applications running in a Docker container.
        /// When executing within a developer environment, the applications settings are first sourced from enfironment
        /// variables and then by the sources configured by the AddAppSettingsAndMachine methods.  When the application
        /// is not executing within the development environment, all variables are sourced from environment variables.
        /// </summary>
        /// <param name="builder">The configuraiton to add setting providers.</param>
        /// <returns>Instance of the configuration builder.</returns>
        public static IConfigurationBuilder AddDockerDefaultSettings(this IConfigurationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder), "Configuration builder not specified.");
            
            builder.AddEnvironmentVariables();
            
            if (EnvironmentConfig.IsDevelopment)
            {
                builder.AddAppSettings();    
            }

            return builder;
        }

        /// <summary>
        /// Shoud be called after adding all settings sources to override any existing configured settings with a set
        /// of in memory values.
        /// </summary>
        /// <param name="builder">The configuration to override with set of values.</param>
        /// <param name="values">The values to add to the configuraiton.</param>
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