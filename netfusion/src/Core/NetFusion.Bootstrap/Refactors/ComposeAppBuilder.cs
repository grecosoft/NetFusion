using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;

namespace NetFusion.Bootstrap.Refactors
{
    public class ComposeAppBuilder : IComposeAppBuilder
    {
        private readonly CompositeAppContainer _container;
        
        public ComposeAppBuilder(IServiceCollection services, 
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            _container = new CompositeAppContainer(services, configuration, loggerFactory, true);
        }
        
        public IComposeAppBuilder AddPlugin<TPlugin>() where TPlugin : IPluginDefinition, new()
        {
            var plugin = new TPlugin();
            _container.AddPlugin(plugin);

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

        public T GetConfig<T>() where T : IContainerConfig
        {
            return _container.GetConfig<T>();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IComposeAppBuilder CompositeAppBuilder(this IServiceCollection services,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            return new ComposeAppBuilder(services, loggerFactory, configuration);
        }
    }
}