using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Container
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a method to the IServiceCollection interface returning a builder
        /// used to create a composite container for a set of plugins.
        /// </summary>
        /// <param name="services">The base service collection provided by the host.</param>
        /// <param name="loggerFactory">The longer factory abstraction provided by the host.</param>
        /// <param name="configuration">The configuration abstraction provided by the host.</param>
        /// <returns>Builder used to create composite-container instance.</returns>
        public static ICompositeContainerBuilder CompositeAppBuilder(this IServiceCollection services,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            return new CompositeContainerBuilder(services, loggerFactory, configuration);
        }
    }
}