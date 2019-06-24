using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;

namespace NetFusion.Builder
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Creates an instance of the ICompositeContainerBuilder builder used to compose
        /// an application from a set of plugins.</summary>
        /// <param name="services">The base service collection provided by the host.</param>
        /// <param name="loggerFactory">The logger factory abstraction provided by the host.</param>
        /// <param name="configuration">The configuration abstraction provided by the host.</param>
        /// <param name="providerFactory">Optional factory method used to specify a specific
        /// dependency-injection container to use.</param>
        /// <returns>Builder used to create composite-container instance.</returns>
        public static ICompositeContainerBuilder CompositeAppBuilder(this IServiceCollection services,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            Func<IServiceCollection, IServiceProvider> providerFactory = null)
        {
            return new CompositeContainerBuilder(services, loggerFactory, configuration);
        }
    }
}