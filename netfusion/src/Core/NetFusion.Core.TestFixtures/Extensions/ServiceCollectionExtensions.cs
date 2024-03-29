using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Logging;
using NetFusion.Core.Bootstrap.Container;

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
    
    public static ICompositeContainerBuilder CompositeContainer(this IServiceCollection services,
        IConfiguration configuration, ITypeResolver resolver,  IExtendedLogger extendedLogger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(extendedLogger);

        NfExtensions.Logger = extendedLogger;
        
        return new CompositeContainerBuilder(services,
            LoggerFactory.Create(_ => { }), 
            configuration, 
            resolver);
    }
}