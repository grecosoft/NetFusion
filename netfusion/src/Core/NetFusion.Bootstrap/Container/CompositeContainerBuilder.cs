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
        
        public CompositeContainerBuilder(IServiceCollection services, 
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _container = new CompositeContainer(services, configuration, loggerFactory, true);
        }
        
        public ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new()
        {
            _container.RegisterPlugin<TPlugin>();
            return this;
        }

        public ICompositeContainerBuilder InitConfig<T>(Action<T> configure) where T : IPluginConfig
        {
            T config = _container.GetContainerConfig<T>();
            configure(config);
            
            return this;
        }

        public IBuiltContainer Build()
        {
            var resolver = new TypeResolver();
            
            _container.Compose(resolver);
            return _container;
        }

        public IServiceProvider Create()
        {
            var sp = _container.CreateServiceProvider();
            ((IBuiltContainer)_container).Start();
            return sp;
        }

        public T GetPluginConfig<T>() where T : IPluginConfig
        {
            return _container.GetPluginConfig<T>();
        }
    }
}