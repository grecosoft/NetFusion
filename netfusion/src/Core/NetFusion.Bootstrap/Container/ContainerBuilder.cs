using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Constructs an instance of an application container.  This class is used during the start of
    /// an application host to populate a service collection in an organized way from a set of plug-in
    /// assemblies containing modules.
    /// </summary>
    public class ContainerBuilder : IContainerBuilder
    {
        // Abstractions of the application's execution environment.
        private IServiceCollection _services;
        private IConfiguration _configuration;
        private ILoggerFactory _loggerFactory;
        private ITypeResolver _typeResolver;
 
        private IAppContainer _container;

        public static IContainerBuilder Create(
            IServiceCollection services, 
            IConfiguration configuration,
            ILoggerFactory loggerFactory, 
            ITypeResolver typeResolver)
        {
            return new ContainerBuilder
            {
                _services = services ?? throw new ArgumentNullException(nameof(services)),
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)),
                _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)),
                _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver))
            };
        }

        public IContainerBuilder Bootstrap(Action<IAppContainer> bootstrap)
        {
            if (bootstrap == null) throw new ArgumentNullException(nameof(bootstrap));

            _container = _container ?? new AppContainer(_services, _configuration, _loggerFactory, _typeResolver);
            bootstrap(_container);
            return this;
        }

        public IBuiltContainer Build()
        {
            _services.AddLogging();
            _services.AddOptions();
            _services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            _services.AddSingleton(typeof(IConfiguration), _configuration);

            var container = _container ?? new AppContainer(_services, _configuration, _loggerFactory, _typeResolver);
            _container = null;

            return container.Build();
        }
    }

    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Creates an application builder associated with a service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration to be used.</param>
        /// <param name="loggerFactory">The logger factory to be used.</param>
        /// <param name="typeResolver">The type resolver to be used.</param>
        /// <returns>Container builder instance.</returns>
        public static IContainerBuilder CreateAppBuilder(this IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            ITypeResolver typeResolver)
        {
            return ContainerBuilder.Create(services, 
                configuration, 
                loggerFactory, 
                typeResolver);
        }
    }
}
