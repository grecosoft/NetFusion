using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Common;
using System;

namespace NetFusion.Bootstrap.Configuration
{
    /// <summary>
    /// Configuration for the overall application environment.  Provides default initialization
    /// of the .NET configuration builder that can be overridden or modified by the host application.
    /// </summary>
    public class EnvironmentConfig : IContainerConfig
    {
        private static string[] CommonEnviromentNameKeys = { "ASPNETCORE_ENVIRONMENT", "NETFUSION_ENVIRONMENT" };

        public IConfiguration _configuration { get; private set; }

        /// <summary>
        /// The value of the variable specifying the environment of the 
        /// executing application.
        /// </summary>
        public static string EnvironmentName => GetEnvironmentName();

        private static string GetEnvironmentName()
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
        /// The created application configuration specified by the host application.
        /// If not specified, a default configured instance is returned.
        /// </summary>
        public IConfiguration Configuration =>
            _configuration ?? new ConfigurationBuilder().AddDefaultAppSettings().Build();

        /// <summary>
        /// Specifies the .NET configuration that should be used by the application container.
        /// </summary>
        /// <param name="builder">A built configuration with providers used to lookup configuration settings.</param>
        public void UseConfiguration(IConfiguration configuration)
        {
            Check.NotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }
    }
}