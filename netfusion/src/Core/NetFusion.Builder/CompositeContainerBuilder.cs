using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Validation;
using NetFusion.Serialization;

namespace NetFusion.Builder
{
    /// <summary>
    /// Provides an API used by the host application to build a composite application
    /// from a set of registered plugins.  The implementation creates and configures
    /// an instance of the CompositeContainer to which plugins are added.
    /// </summary>
    public class CompositeContainerBuilder : ICompositeContainerBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IBootstrapLogger _bootstrapLogger;
        private readonly ITypeResolver _typeResolver;
        
        private readonly CompositeContainer _container;
        
        public CompositeContainerBuilder(IServiceCollection serviceCollection, 
            IConfiguration configuration,
            IBootstrapLogger bootstrapLogger,
            ITypeResolver typeResolver)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _bootstrapLogger = bootstrapLogger ?? throw new ArgumentNullException(nameof(bootstrapLogger));
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            
            _container = new CompositeContainer(serviceCollection, configuration, bootstrapLogger);
        }
        
        public ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new()
        {
            _container.RegisterPlugin<TPlugin>();
            return this;
        }

        // This override is used exclusively by unit tests.
        public ICompositeContainerBuilder AddPlugin(params IPlugin[] plugin)
        {
            _container.RegisterPlugins(plugin);
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

        // Populates the IServiceCollection with services registered by all plugin-modules.
        // The end result is a populated service-collection with a registered ICompositeApp
        // instance that can be used for the lifetime of the host.  
        public void Compose(Action<IServiceCollection> config = null)
        {
            RegisterRequiredDefaultServices();

            try
            {
                // Not until the ICompositeApp has been created will the service-provider
                // be created and the ILogger available.  Until this point, all logs are
                // written to the IBootstrapLogger.  
                _container.Compose(_typeResolver);
                
                // Account for the case where a message with an Error log level is recorded
                // for which an exception was not raised.
                if (_bootstrapLogger.HasErrors)
                {
                    throw new ContainerException(
                        "Errors were recorded when composing application.  See log for details.");
                }
                
                // Allow the host initialization code to specify any last services.
                config?.Invoke(_serviceCollection);
            }
            catch 
            {
                _bootstrapLogger.WriteToStandardOut();
                throw;
            }
        }

        private void RegisterRequiredDefaultServices()
        {
            _bootstrapLogger.Add(LogLevel.Debug, 
                "Adding Required Default Services");
            
            RegisterDefaultService(typeof(ILoggerFactory), typeof(LoggerFactory));  
            
            // These services can be overridden by the host.
            RegisterDefaultService(typeof(IEntityScriptingService), typeof(NullEntityScriptingService));
            RegisterDefaultService(typeof(IValidationService), typeof(ValidationService));
            RegisterDefaultService(typeof(ISerializationManager), typeof(SerializationManager));
        }
        
        private void RegisterDefaultService(Type serviceType, Type implementationType)
        {
            _serviceCollection.AddSingleton(serviceType, implementationType);
            
            _bootstrapLogger.Add(LogLevel.Debug, 
                $"Service: {serviceType}; Implementation: {implementationType} ");
        }
    }
}