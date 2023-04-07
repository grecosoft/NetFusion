using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace NetFusion.Core.TestFixtures.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AssertHasRegistration<TService, TImplementation>(this IServiceCollection services,
        ServiceLifetime lifetime)
    {
        Assert.True(
            services.Count(s =>
                s.ServiceType == typeof(TService) &&
                s.ImplementationType == typeof(TImplementation) &&
                s.Lifetime == lifetime) == 1,
            $"Service type {typeof(TService)} with implementation {typeof(TImplementation)} not found");
    }
    
    public static void AssertHasRegistration<TImplementation>(this IServiceCollection services,
        ServiceLifetime lifetime)
    {
        Assert.True(
            services.Count(s =>
                s.ImplementationType == typeof(TImplementation) &&
                s.Lifetime == lifetime) == 1,
            $"Service implementation {typeof(TImplementation)} not found");
    }
}