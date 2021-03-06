using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Logging;
using NetFusion.Bootstrap.Container;
// ReSharper disable InvalidXmlDocComment

namespace NetFusion.Builder
{
    /// <summary>
    /// Contains extension methods for IServiceCollection invoked by the host application.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Creates an instance of the ICompositeContainerBuilder used to compose an
        /// application from a set of plugins.
        /// 
        /// <example>
        ///     services.CompositeContainer(_configuration, new SerilogExtendedLogger())
        ///         .AddMessaging()
        ///
        ///         .AddPlugin<InfraPlugin>()
        ///         .AddPlugin<AppPlugin>()
        ///         .AddPlugin<DomainPlugin>()
        ///         .AddPlugin<WebApiPlugin>()
        ///         .Compose();
        /// </example>
        /// </summary>
        /// <param name="services">The base service collection provided by the host.</param>
        /// <param name="configuration">The host application's configuration.</param>
        /// <param name="extendedLogger">Logger specified by the host providing extended logging
        /// to Microsoft's ILogger.  This logger is also used during the bootstrap process before
        /// Microsoft's ILogger is available via dependency injection.</param>
        /// <returns>Builder used to create composite-container instance.</returns>
        public static ICompositeContainerBuilder CompositeContainer(this IServiceCollection services,
            IConfiguration configuration, IExtendedLogger extendedLogger)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (extendedLogger == null) throw new ArgumentNullException(nameof(extendedLogger));
            
            var resolver = new TypeResolver(extendedLogger);
            return new CompositeContainerBuilder(services, configuration, resolver, extendedLogger);
        }
    }
}