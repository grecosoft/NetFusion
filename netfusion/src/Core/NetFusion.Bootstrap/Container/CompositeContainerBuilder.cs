using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Provides an API used by the host application to build a composite application
    /// from a set of registered plugins.
    /// </summary>
    public class CompositeContainerBuilder : ICompositeContainerBuilder
    {
        private readonly CompositeContainer _container;
        
        internal CompositeContainerBuilder(IServiceCollection services, 
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _container = new CompositeContainer(services, configuration, loggerFactory, true);
        }

        internal void SetProviderFactory(Func<IServiceCollection, IServiceProvider> providerFactory)
        {
            if (providerFactory == null) throw new ArgumentNullException(nameof(providerFactory));
            _container.SetProviderFactory(providerFactory);
        }
        
        public ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new()
        {
            _container.RegisterPlugin<TPlugin>();
            return this;
        }

        public ICompositeContainerBuilder InitConfig<T>(Action<T> configure) where T : IPluginConfig
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            
            T config = _container.GetContainerConfig<T>();
            configure(config);
            
            return this;
        }
        
        public T GetPluginConfig<T>() where T : IPluginConfig
        {
            return _container.GetPluginConfig<T>();
        }

        public ICompositeContainer Build()
        {
            var resolver = new TypeResolver();
            
            _container.Compose(resolver);
            return _container;
        }
    }
}