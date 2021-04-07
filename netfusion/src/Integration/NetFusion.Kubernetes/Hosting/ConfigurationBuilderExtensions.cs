using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NetFusion.Kubernetes.Hosting
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddJsonFiles(this IConfigurationBuilder builder, 
            IHostEnvironment hostEnvironment,
            KubeConfigOptions options)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (hostEnvironment == null) throw new ArgumentNullException(nameof(hostEnvironment));
            if (options == null) throw new ArgumentNullException(nameof(options));
            
            options.Validate();

            // If the host is running locally load the configuration for the specified 
            // local configuration directory.  Most often these are the configurations
            // used when running local and not within Kubernetes.
            if (hostEnvironment.IsDevelopment())
            {
                string localConfigPath = Path.Join(Directory.GetCurrentDirectory(), options.LocalConfigPath);
                AddJsonFiles(builder, localConfigPath, options.ReloadOnChange);

                return builder;
            }
            
            // All other environments load from container directories mapped to Kubernetes volumes.  
            // This directory is the same for all environments above Development.  The build,
            // determines which environment files are used to create the associated Kubernetes
            // ConfigMap and Secret resources.
            AddJsonFiles(builder, options.ContainerConfigPath, options.ReloadOnChange);
            return builder;
        }

        private static void AddJsonFiles(IConfigurationBuilder builder, string configDirectory, bool reloadOnChange)
        {
            if (! Directory.Exists(configDirectory))
            {
                Console.Error.WriteLine($"The specified configuration directory: {configDirectory} does not exist.");
                return;
            }
            
            foreach (string file in Directory.GetFiles(configDirectory, "*.json"))
            {
                Console.WriteLine($"Loading Configuration: {file}");
                builder.AddJsonFile(file, true, reloadOnChange);
            }
        }
    }
}