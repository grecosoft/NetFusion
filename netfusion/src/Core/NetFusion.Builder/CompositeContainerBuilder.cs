using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Container;
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
        
        internal CompositeContainerBuilder(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _container = new CompositeContainer(serviceCollection, configuration);
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

        public void Compose()
        {
            var resolver = new TypeResolver();
            
            RegisterCommonContainerServices();

            try
            {
                // Not until the ICompositeApp has been created will the service-provider
                // be created and the ILogger available.  Until this point, all logs are
                // written to the IBootstrapLogger.  This is new from .net core 3.0 forward.
                _container.Compose(resolver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                _container.BootstrapLogger.WriteToStandardOut();

                 throw;
            }
        }

        private void RegisterCommonContainerServices()
        {
            _container.BootstrapLogger.Add(LogLevel.Information, 
                "Adding NetFusion Required and Default Services");
            
            RegisterDefaultService(typeof(LoggerFactory));
            RegisterDefaultService(typeof(IEntityScriptingService), typeof(NullEntityScriptingService));
            RegisterDefaultService(typeof(IValidationService), typeof(ValidationService));
            RegisterDefaultService(typeof(ISerializationManager), typeof(SerializationManager));
        }
        
        private void RegisterDefaultService(Type implementationType)
        {
            _serviceCollection.AddSingleton(implementationType);
            
            _container.BootstrapLogger.Add(LogLevel.Information, 
                $"Implementation: {implementationType} ");
        }

        private void RegisterDefaultService(Type serviceType, Type implementationType)
        {
            _serviceCollection.AddSingleton(serviceType, implementationType);
            
            _container.BootstrapLogger.Add(LogLevel.Information, 
                $"Service: {serviceType}; Implementation: {implementationType} ");
        }
    }
}