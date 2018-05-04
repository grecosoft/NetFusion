using NetFusion.Bootstrap.Container;
using System;

namespace NetFusion.Bootstrap.Configuration
{
    /// <summary>
    /// Configuration for the overall application environment.  Provides default initialization
    /// of the .NET configuration extensions that can be overridden or modified by the host application.
    /// </summary>
    public class EnvironmentConfig : IContainerConfig
    {
        private static string[] CommonEnviromentNameKeys = { "NETFUSION_ENVIRONMENT", "ASPNETCORE_ENVIRONMENT" };

        /// <summary>
        /// The value of the variable specifying the environment of the executing application.
        /// </summary>
        public static string EnvironmentName => GetEnvironmentName();

        private static string GetEnvironmentName()
        {
            foreach (string key in CommonEnviromentNameKeys)
            {
                string value = Environment.GetEnvironmentVariable(key);
                if (! string.IsNullOrWhiteSpace(value))
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
    }
}