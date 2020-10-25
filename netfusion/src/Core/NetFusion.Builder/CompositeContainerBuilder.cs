using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base;
using NetFusion.Base.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
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
        private readonly ITypeResolver _typeResolver;

        private readonly CompositeContainer _container;
        
        public CompositeContainerBuilder(IServiceCollection serviceCollection,
            IConfiguration configuration,
            ITypeResolver typeResolver,
            IExtendedLogger extendedLogger = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));

            if (extendedLogger != null)
            {
                NfExtensions.Logger = extendedLogger;
            }
            
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
        // The end result is a populated service-collection with a registered ICompositeApp
        // instance that can be used for the lifetime of the host.  
        public void Compose(Action<IServiceCollection> config = null)
        {
            try
            {
                RegisterRequiredDefaultServices();

                _container.Compose(_typeResolver);
                
                // Allow the host initialization code to specify any last service overrides.
                config?.Invoke(_serviceCollection);
            }
            catch(Exception ex)
            {
                NfExtensions.Logger.Error<CompositeContainerBuilder>(ex, "Error building Composite Container");
                throw;
            }
        }
        
        private void RegisterRequiredDefaultServices()
        {
            _serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();

            // These services can be overridden by the host:
            _serviceCollection.AddSingleton<IValidationService, ValidationService>();
            _serviceCollection.AddSingleton<ISerializationManager, SerializationManager>();
            _serviceCollection.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
        }
    }
}