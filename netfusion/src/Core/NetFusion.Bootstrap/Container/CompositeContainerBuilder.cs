using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    public class CompositeContainerBuilder : ICompositeContainerBuilder
    {
        private readonly CompositeContainer _container;
        
        public CompositeContainerBuilder(IServiceCollection services, 
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
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
            
            _container.Build(resolver);
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

    public static class ServiceCollectionExtensions
    {
        public static ICompositeContainerBuilder CompositeAppBuilder(this IServiceCollection services,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            return new CompositeContainerBuilder(services, loggerFactory, configuration);
        }
    }
}