using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NetFusion.Kubernetes.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Extension method invoked during the building of the Host to initialize
        /// configuration sources most appropriate for running a Microservice within
        /// Kubernetes. 
        /// </summary>
        /// <param name="builder">Reference to class used to construct the host.</param>
        /// <param name="configBuilder">Delegate passed a reference to IConfiguration Builder
        /// called just before the Environment and CommandLine sources are added to override
        /// any prior determined configuration values.</param>
        /// <param name="args">Reference to the command line arguments passed to the host.</param>
        /// <returns>Reference to Host Builder.</returns>
        public static IHostBuilder AddKubernetesConfigConventions(this IHostBuilder builder,
            Action<IConfigurationBuilder> configBuilder,
            string[] args = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configBuilder == null) throw new ArgumentNullException(nameof(configBuilder));

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Properties.Clear();
                
                AddDevConfigSources(hostingContext, config);
                configBuilder(config);
                AddOverrideConfigSources(config, args);
            });
            
            return builder;
        }

        private static void AddDevConfigSources(HostBuilderContext hostContext, 
            IConfigurationBuilder config)
        {
            bool reloadOnChange = hostContext.Configuration.GetValue(
                "hostBuilder:reloadConfigOnChange", 
                defaultValue: true);

            // When running within Development the service's settings are read
            // from the application settings file:
            IHostEnvironment env = hostContext.HostingEnvironment;
            if (env.IsDevelopment())
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange);
                if (!string.IsNullOrEmpty(env.ApplicationName))
                {
                    var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                    config.AddUserSecrets(appAssembly, optional: true);
                }
            }
        }

        private static void  AddOverrideConfigSources(IConfigurationBuilder config, string[] args)
        {
            config.AddEnvironmentVariables();
            if (args != null)
            {
                config.AddCommandLine(args);
            }
        }

        /// <summary>
        /// Builder extension method used to add all json configuration files within
        /// a directory as file configuration sources.
        /// </summary>
        /// <param name="builder">Reference to configuration builder.</param>
        /// <param name="mountedDirectory">Container directory path mounted to a Kubernetes ConfigMap volume.</param>
        /// <param name="reloadOnChange">Determines if the file should be reloaded if changed.</param>
        public static void AddMountedVolumeJsonFiles(this IConfigurationBuilder builder, 
            string mountedDirectory, 
            bool reloadOnChange = false)
        {
            if (string.IsNullOrWhiteSpace(mountedDirectory))
                throw new ArgumentException("Mounted directory path not specified.", nameof(mountedDirectory));
            
            if (! Directory.Exists(mountedDirectory))
            {
                Console.Error.WriteLine($"The specified mounted directory: {mountedDirectory} does not exist.");
                return;
            }
            
            foreach (string file in Directory.GetFiles(mountedDirectory, "*.json"))
            {
                Console.WriteLine($"Loading Configuration: {file}");
                builder.AddJsonFile(file, true, reloadOnChange);
            }
        }
    }
}