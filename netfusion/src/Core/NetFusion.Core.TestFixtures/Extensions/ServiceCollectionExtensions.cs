using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace NetFusion.Core.TestFixtures.Extensions;

public static class ServiceCollectionExtensions
{
    public static bool HasRegistration<TService, TImplementation>(this IServiceCollection services,
        ServiceLifetime lifetime)
    {
        return services.Count(s =>
                s.ServiceType == typeof(TService) &&
                s.ImplementationType == typeof(TImplementation) &&
                s.Lifetime == lifetime) == 1;
    }
    
    public static bool HasRegistration<TImplementation>(this IServiceCollection services,
        ServiceLifetime lifetime)
    {
        return services.Count(s =>
            s.ImplementationType == typeof(TImplementation) &&
            s.Lifetime == lifetime) == 1;

    }
}