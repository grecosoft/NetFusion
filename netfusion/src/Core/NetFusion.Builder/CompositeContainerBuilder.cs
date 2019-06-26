using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Container;
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
        
        internal CompositeContainerBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            
            RegisterCommonContainerServices(serviceCollection);
            
            _container = new CompositeContainer(serviceCollection);
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

        public void Compose(Action<IServiceCollection> register = null)
        {
            var resolver = new TypeResolver();
            
            _container.Compose(resolver);
            
            // Allow the host to override any services:
            register?.Invoke(_serviceCollection);
        
            //LogBuilderConfig(composite.ServiceProvider);
        }

        private static void RegisterCommonContainerServices(IServiceCollection services)
        {
            services.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<ISerializationManager, SerializationManager>();
        }

//        private void LogBuilderConfig(IServiceProvider serviceProvider)
//        {
//            var serializationMgr = serviceProvider.GetService<ISerializationManager>();
//
//            if (serializationMgr == null || !_logger.IsEnabled(LogLevel.Debug))
//            {
//                return;
//            }
//
//            var details = new
//            {
//                SerializationManager = serializationMgr.GetType().FullName,
//                Serializers = serializationMgr.Serializers.Select(s => new {
//                    s.ContentType,
//                    SerializerType = s.GetType().FullName
//                }).ToArray()
//            };
//
//            _logger.LogDebug(details.ToIndentedJson());
//        }
    }
}