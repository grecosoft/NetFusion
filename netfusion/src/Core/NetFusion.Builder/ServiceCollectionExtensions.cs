using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Logging;
using NetFusion.Bootstrap;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Logging;

namespace NetFusion.Builder
{
    /// <summary>
    /// Contains extension methods used to build a composite-container by the host application. 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Creates an instance of the ICompositeContainerBuilder used to compose
        /// an application from a set of plugins.</summary>
        /// <param name="services">The base service collection provided by the host.</param>
        /// <param name="configuration">The application's configuration.</param>
        /// <returns>Builder used to create composite-container instance.</returns>
        public static ICompositeContainerBuilder CompositeContainer(this IServiceCollection services,
            IConfiguration configuration, IExtendedLogger extendedLogger = null)
        {
            NfExtensions.Logger = extendedLogger ?? NfExtensions.Logger;
            
            var resolver = new TypeResolver(extendedLogger);
            return new CompositeContainerBuilder(services, configuration, resolver, extendedLogger);
        }
    }
}