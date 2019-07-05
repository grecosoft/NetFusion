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
    /// from a set of registered plugins.
    /// </summary>
    public class CompositeContainerBuilder : ICompositeContainerBuilder
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly CompositeContainer _container;
        private readonly ITypeResolver _typeResolver;
        
        public CompositeContainerBuilder(IServiceCollection serviceCollection, 
            ITypeResolver typeResolver,
            IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            _container = new CompositeContainer(serviceCollection, configuration);
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
        // the end result is a populated service-collection with a register ICompositeApp
        // that can be used for the lifetime of the host.  The associated IServiceProvider
        // has not yet been created.
        public void Compose(Action<IServiceCollection> config = null)
        {
            RegisterRequiredDefaultServices();

            try
            {
                // Not until the ICompositeApp has been created will the service-provider
                // be created and the ILogger available.  Until this point, all logs are
                // written to the IBootstrapLogger.  This is new from .net core 3.0 forward.
                _container.Compose(_typeResolver);
                
                // Account for the case where a message with an Error log level is recorded
                // for which an exception was not raised.
                if (_container.BootstrapLogger.HasErrors)
                {
                    _container.BootstrapLogger.WriteToStandardOut();
                    
                    throw new ContainerException(
                        "Errors were recorded when composing application.  See log for details.");
                }
                
                // Allow the host initialization code to specify any last services.
                config?.Invoke(_serviceCollection);
            }
            catch (Exception ex)
            {
                _container.BootstrapLogger.Add(LogLevel.Error, ex.ToString());
                _container.BootstrapLogger.WriteToStandardOut();

                 throw;
            }
        }

        private void RegisterRequiredDefaultServices()
        {
            _container.BootstrapLogger.Add(LogLevel.Debug, 
                "Adding NetFusion Required Default Services");
            
            RegisterDefaultService(typeof(LoggerFactory));
            RegisterDefaultService(typeof(IEntityScriptingService), typeof(NullEntityScriptingService));
            RegisterDefaultService(typeof(IValidationService), typeof(ValidationService));
            RegisterDefaultService(typeof(ISerializationManager), typeof(SerializationManager));
        }
        
        private void RegisterDefaultService(Type implementationType)
        {
            _serviceCollection.AddSingleton(implementationType);
            
            _container.BootstrapLogger.Add(LogLevel.Debug, 
                $"Implementation: {implementationType} ");
        }

        private void RegisterDefaultService(Type serviceType, Type implementationType)
        {
            _serviceCollection.AddSingleton(serviceType, implementationType);
            
            _container.BootstrapLogger.Add(LogLevel.Debug, 
                $"Service: {serviceType}; Implementation: {implementationType} ");
        }
    }
}