using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Logging;

namespace NetFusion.Serilog
{
    public static class ServiceCollectionExtensions
    {
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