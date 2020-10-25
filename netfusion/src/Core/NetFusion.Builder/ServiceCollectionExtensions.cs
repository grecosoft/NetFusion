using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base;
using NetFusion.Base.Logging;
using NetFusion.Bootstrap.Container;

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
        /// <param name="extendedLogger">Logger specified by the host providing extended logging
        /// to Microsoft's ILogger.  This logger is also used during the bootstrap process before
        /// Microsoft's ILogger is available via dependency injection.</param>
        /// <returns>Builder used to create composite-container instance.</returns>
        public static ICompositeContainerBuilder CompositeContainer(this IServiceCollection services,
            IConfiguration configuration, IExtendedLogger extendedLogger = null)
        {
            var resolver = new TypeResolver(extendedLogger ?? NfExtensions.Logger);
            return new CompositeContainerBuilder(services, configuration, resolver, extendedLogger);
        }
    }
}