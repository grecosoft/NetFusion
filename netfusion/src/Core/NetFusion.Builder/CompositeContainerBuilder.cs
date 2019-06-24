using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Validation;
using NetFusion.Common.Extensions;
using NetFusion.Serialization;

namespace NetFusion.Builder
{
    /// <summary>
    /// Provides an API used by the host application to build a composite application
    /// from a set of registered plugins.
    /// </summary>
    public class CompositeContainerBuilder : ICompositeContainerBuilder
    {
        private readonly CompositeContainer _container;
        private readonly IComposite _composite;
        private readonly ILogger _logger;
        
        internal CompositeContainerBuilder(IServiceCollection services, 
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _logger = loggerFactory.CreateLogger<CompositeContainerBuilder>();
            
            RegisterCommonContainerServices(services);
            
            _container = new CompositeContainer(services, configuration, loggerFactory, true);
            _composite = _container;
        }
        
        public ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new()
        {
            _container.RegisterPlugin<TPlugin>();
            return this;
        }

        public ICompositeContainerBuilder InitContainerConfig<T>(Action<T> configure) where T : IContainerConfig
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            
            T config = _container.GetContainerConfig<T>();
            configure(config);
            
            return this;
        }

        public ICompositeContainerBuilder InitPluginConfig<T>(Action<T> configure) where T : IPluginConfig
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            T config = _container.GetPluginConfig<T>();
            configure(config);

            return this;
        }

        public ICompositeContainer Build(Action<IServiceCollection> services = null)
        {
            var resolver = new TypeResolver();
            
            IComposite composite = _container.Compose(resolver);
            
            // Allow the host to override any services:
            services?.Invoke(_composite.Services);
            
            // Create the service provider:
            composite.CreateServiceProvider();

            LogBuilderConfig(composite.ServiceProvider);
            
            return _container;
        }

        private static void RegisterCommonContainerServices(IServiceCollection services)
        {
            services.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<ISerializationManager, SerializationManager>();
        }

        private void LogBuilderConfig(IServiceProvider serviceProvider)
        {
            var serializationMgr = serviceProvider.GetService<ISerializationManager>();

            if (serializationMgr == null || !_logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var details = new
            {
                SerializationManager = serializationMgr.GetType().FullName,
                Serializers = serializationMgr.Serializers.Select(s => new {
                    s.ContentType,
                    SerializerType = s.GetType().FullName
                }).ToArray()
            };

            _logger.LogDebug(details.ToIndentedJson());
        }
    }
}