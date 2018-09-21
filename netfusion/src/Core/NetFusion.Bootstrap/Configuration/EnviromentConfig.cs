using System;
using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Configuration
{
    /// <summary>
    /// Configuration for the overall application environment.  
    /// </summary>
    public class EnvironmentConfig 
    {
        private static readonly string[] CommonEnviromentNameKeys =
        {
            "NETFUSION_ENVIRONMENT", 
            "ASPNETCORE_ENVIRONMENT"
        };
        
        /// <summary>
        /// Commonly used environment names.
        /// </summary>
        public static class Names
        {
            public const string Development = "Development";
            public const string Test = "Test";
            public const string Production = "Production";
        }

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

            return Names.Development;
        }

        /// <summary>
        /// Indicates application is running in Development environment.
        /// </summary>
        public static bool IsDevelopment => EnvironmentName.Equals(Names.Development, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates application is running in Test environment.
        /// </summary>
        public static bool IsTest => EnvironmentName.Equals(Names.Test, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Indicates application is running in Production environment.
        /// </summary>
        public static bool IsProduction => EnvironmentName.Equals(Names.Production, StringComparison.OrdinalIgnoreCase);
        
        /// <summary>
        /// Determines the log level based on the execution environment:
        /// - Development: Trace,
        /// - Test: Debug,
        /// - Production: Warning
        /// </summary>
        public static LogLevel EnvironmentMinLogLevel =>
            IsDevelopment ? LogLevel.Debug
            : IsTest ? LogLevel.Debug
            : IsProduction ? LogLevel.Warning
            : LogLevel.Information;
    }
}