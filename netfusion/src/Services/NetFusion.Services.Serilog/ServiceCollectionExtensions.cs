using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Base.Logging;

namespace NetFusion.Services.Serilog;

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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logLevelControl);

        services.AddSingleton(logLevelControl);
        return services;
    }
}