using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace NetFusion.Builder.Kubernetes;

public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Called when configuring the host for a microservice to read configurations and secrets
    /// from volumes mounted to Kubernetes ConfigMaps and Secrets.  If the specified configuration
    /// mounted path exists, all existing configuration providers are cleared so configurations and
    /// secrets are exclusively loaded from all the mounted Kubernetes volumes.  
    /// </summary>
    /// <param name="configurationBuilder">Reference to the host's configuration builder.</param>
    /// <param name="configMountPath">The mounted path containing configurations.</param>
    /// <param name="secretMountPath">The optional mounted path containing secrets.</param>
    /// <returns>Returns to configuration builder.</returns>
    public static IConfigurationBuilder AddVolumeMounts(
        this IConfigurationBuilder configurationBuilder,
        string configMountPath,
        string secretMountPath = null)
    {
        if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));
        
        if (string.IsNullOrWhiteSpace(configMountPath))
            throw new ArgumentException("Configuration mount path not specified.", nameof(configMountPath));

        var configMountDirectory = new DirectoryInfo(configMountPath);
        if (!configMountDirectory.Exists)
        {
            return configurationBuilder;
        }
        
        // Clear all existing sources so development settings are not accidentally
        // used.  Note, the Docker image containing the microservice should never
        // include configurations - should be listed in .dockerignore.
        configurationBuilder.Sources.Clear();
        
        AddConfigurationFiles(configurationBuilder, configMountDirectory);

        if (! string.IsNullOrWhiteSpace(secretMountPath))
        {
            var secretMountDirectory = new DirectoryInfo(secretMountPath);
            AddConfigurationFiles(configurationBuilder, secretMountDirectory);
        }
        
        // Add environment variables last so they can override any loaded configurations
        // loaded from file:
        configurationBuilder.AddEnvironmentVariables();
        return configurationBuilder;
    }
    
    private static void AddConfigurationFiles(IConfigurationBuilder configurationBuilder,
        DirectoryInfo mountedDirectory)
    {
        if (! mountedDirectory.Exists)
        {
            return;
        }
        
        foreach (var configFile in mountedDirectory.GetFiles("*.json"))
        {
            configurationBuilder.AddJsonFile(configFile.FullName);
        }
    }
}