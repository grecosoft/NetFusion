using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Common;
using System;
using System.IO;

namespace NetFusion.Bootstrap.Configuration
{
    /// <summary>
    /// Configuration for the overall application environment.  Provides default initialization
    /// of the .NET configuration builder that can be overridden or modified by the host application.
    /// </summary>
    public class EnviromentConfig : IContainerConfig
    {
        private const string APP_SETTINGS_FILE_NAME = "appsettings.json";
        private static string[] CommonEnviromentNameKeys = { "ASPNETCORE_ENVIRONMENT", "NETFUSION_ENVIRONMENT" };

        public string EnvironmentName { get; }
        public IConfigurationBuilder ConfigurationBuilder { get; private set; }

        /// <summary>
        /// Indicates if the environment variable configuration provider should be added.
        /// </summary>
        public bool AddEnvironmentVariables { get; set; } = true;

        public EnviromentConfig()
        {
            EnvironmentName = GetEnvironmentName();
            ConfigurationBuilder = new ConfigurationBuilder();
        }

        private string GetEnvironmentName()
        {
            foreach (string key in CommonEnviromentNameKeys)
            {
                string value = Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return EnvironmentNames.Development;
        }

        /// <summary>
        /// Specifies the .NET configuration builder that should be used by the application container.
        /// </summary>
        /// <param name="builder">A configured builder with providers used to lookup configuration settings.</param>
        public void UseConfiguration(IConfigurationBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            ConfigurationBuilder = builder;
        }

        /// <summary>
        /// Creates a new uninitialized .NET configuration builder instance that can be 
        /// configured by the passed delegate.
        /// </summary>
        /// <param name="config">Delegate used to configure the configuration builder.</param>
        public void UseConfiguration(Action<IConfigurationBuilder> config)
        {
            Check.NotNull(config, nameof(config));

            ConfigurationBuilder = new ConfigurationBuilder();
            config(ConfigurationBuilder);
        }

        /// <summary>
        /// Creates a new .NET configuration builder instance initialized with a common
        /// set of default conventions.
        /// </summary>
        /// <param name="config">Delegate used to configure the configuration builder.</param>
        public void UseDefaultConfiguration(Action<IConfigurationBuilder> config = null)
        {
            ConfigurationBuilder = CreateDefaultConfiguration();

            config?.Invoke(ConfigurationBuilder);

            // Add optional configuration providers.
            if (AddEnvironmentVariables)
            {
                ConfigurationBuilder.AddEnvironmentVariables();
            }
        }

        private IConfigurationBuilder CreateDefaultConfiguration()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
    
            configBuilder.SetBasePath(Directory.GetCurrentDirectory());
            configBuilder.AddJsonFile(APP_SETTINGS_FILE_NAME, optional: true, reloadOnChange: true);
            configBuilder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.{EnvironmentName}.json", reloadOnChange: true, optional: true);
            configBuilder.AddJsonFile($"{APP_SETTINGS_FILE_NAME}.{Environment.MachineName}.json", reloadOnChange: true, optional: true);
            
            return configBuilder;
        }
    }
}