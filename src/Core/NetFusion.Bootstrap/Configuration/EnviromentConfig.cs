using Microsoft.Extensions.Configuration;
using NetFusion.Bootstrap.Container;
using System;

namespace NetFusion.Bootstrap.Configuration
{
    /// <summary>
    /// Configuration for the overall application environment.  Provides default initialization
    /// of the .NET configuration that can be overridden or modified by the host application.
    /// </summary>
    public class EnvironmentConfig : IContainerConfig
    {
        private static string[] CommonEnviromentNameKeys = { "NETFUSION_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT" };

        private IConfiguration _configuration { get; set; }

        /// <summary>
        /// The value of the variable specifying the environment of the executing application.
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
        /// Indicates application is running in Development environment.
        /// </summary>
        public static bool IsDevelopment => EnvironmentName.Equals(EnvironmentNames.Development, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates application is running in Staging environment.
        /// </summary>
        public static bool IsStaging => EnvironmentName.Equals(EnvironmentNames.Staging, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates application is running in Test environment.
        /// </summary>
        public static bool IsTest => EnvironmentName.Equals(EnvironmentNames.Test, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates application is running in Production environment.
        /// </summary>
        public static bool IsProduction => EnvironmentName.Equals(EnvironmentNames.Production, StringComparison.OrdinalIgnoreCase);

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
            _configuration = configuration
                ?? throw new ArgumentNullException(nameof(configuration), "Configuration must be specified.");
        }
    }
}