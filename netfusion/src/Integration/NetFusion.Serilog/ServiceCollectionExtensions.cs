using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Logging;

namespace NetFusion.Serilog
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds to the service collection a service used to change the minimum log level at runtime.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="logLevelControl">The service used to change the log level.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddLogLevelControl(this IServiceCollection services,
            ILogLevelControl logLevelControl)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (logLevelControl == null) throw new ArgumentNullException(nameof(logLevelControl));

            services.AddSingleton(logLevelControl);
            return services;
        }
    }
}