using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;

namespace NetFusion.Builder
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Creates an instance of the ICompositeContainerBuilder used to compose
        /// an application from a set of plugins.</summary>
        /// <param name="services">The base service collection provided by the host.</param>
        /// <param name="configuration">The application's configuration.</param>
        /// <returns>Builder used to create composite-container instance.</returns>
        public static ICompositeContainerBuilder CompositeContainer(this IServiceCollection services,
            IConfiguration configuration)
        {
            return new CompositeContainerBuilder(services, configuration);
        }
    }
}