using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Validation;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Responsible for coordinating the population of a Service Collection
    /// from a set of plugins.  
    /// </summary>
    public class CompositeContainer : ICompositeContainer,
        IBuiltContainer
    {
        // Singleton instance of created container:
        private static CompositeContainer _instance;
        private bool _disposed;

        // Microsoft Common Abstractions:
        private readonly IServiceCollection _serviceCollection;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
       
        // Logging implementations:
        private readonly ILogger _logger;
        private CompositeAppLog _compositeLog;
       
        private CompositeApp _compositeApp;
        private readonly List<IPlugin> _plugins = new List<IPlugin>();
       
        private ValidationConfig _validationConfig;
        
        // Service Provider:
        private IServiceProvider _serviceProvider;

        public CompositeContainer(
            IServiceCollection services,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            bool setGlobalReference)
        {
            _serviceCollection = services ?? throw new ArgumentNullException(nameof(services));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            
            _logger = _loggerFactory.CreateLogger<CompositeContainer>();
            
            if (setGlobalReference)
            {
                _instance = this;
            }
        }
        
        // Log of the composite application structure showing how it was constructed.
        public IDictionary<string, object> Log
        {
            get
            {
                ThrowIfDisposed(this);
                return _compositeLog?.GetLog() ?? new Dictionary<string, object>();
            }
        }
        
        /// <summary>
        /// Reference to the singleton application container instance.  This should only
        /// be used when service-locator is necessary from a root component.  If a given
        /// service implementation is contained within the service collection, it can 
        /// reference the application container by injecting the IAppContainer interface.
        /// </summary>
        public static ICompositeContainer Instance
        {
            get
            {
                ThrowIfDisposed(_instance);
                return _instance;
            }
        }
        
        // NOTE:  Developers should not access this property but rather call the CreateServiceScope or
        // ExecuteInServiceScope methods.
        IServiceProvider IBuiltContainer.ServiceProvider
        {
            get
            {
                ThrowIfDisposed(this);
                return _serviceProvider;
            }
        }
        
        internal void AddPlugin(IPlugin plugin)
        {
            _plugins.Add((plugin));
        }
        
        //----------------------- Container Initialization ------------------------------------//
        
        public IBuiltContainer Build(ITypeResolver typeResolver)
        {
            ConfigureValidation();
            
            _compositeApp = new CompositeApp(_loggerFactory, _configuration, _plugins);
            
            try
            {
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapBuild, "Building Container"))
                {
                    ResolvePlugins(typeResolver);
           
                    _compositeApp.Validate();
                    
                    ComposeCorePlugins(typeResolver);
                    ComposeApplicationPlugins(typeResolver);
                    
                    LogPlugins(_compositeApp.AllPlugins);

                    PopulateServiceCollection();
                    CreateCompositeLogger();
                }
            }
            catch (ContainerException ex)
            {
                LogException(ex);
                throw;
            }
            catch (Exception ex)
            {
                throw LogException(new ContainerException(
                    "Unexpected container error.  See Inner Exception.", ex));
            }

            return this;
        }

        public IServiceProvider CreateServiceProvider(
            Func<IServiceCollection, IServiceProvider> providerFactory = null)
        {
            _serviceProvider = providerFactory?.Invoke(_serviceCollection) ?? _serviceCollection.BuildServiceProvider(true);
            return _serviceProvider;
        }

        private void ResolvePlugins(ITypeResolver typeResolver)
        {           
            foreach (IPlugin plugin in _compositeApp.AllPlugins)
            {
                typeResolver.SetPluginMeta(plugin);
            }
        }

        private void ComposeCorePlugins(ITypeResolver typeResolver)
        {
            var allPluginTypes = _compositeApp.GetPluginTypes().ToArray();

            foreach (var plugin in _compositeApp.CorePlugins)
            {
                typeResolver.ComposePlugin(plugin, allPluginTypes);
            }
        }

        private void ComposeApplicationPlugins(ITypeResolver typeResolver)
        {
            var allAppPluginTypes = _compositeApp.GetPluginTypes(
                PluginTypes.ApplicationPlugin, 
                PluginTypes.HostPlugin).ToArray();

            foreach (var plugin in _compositeApp.AppPlugins)
            {
                typeResolver.ComposePlugin(plugin, allAppPluginTypes);
            }
            
            typeResolver.ComposePlugin(_compositeApp.HostPlugin, allAppPluginTypes);
        }

        private void PopulateServiceCollection()
        {
            _compositeApp.PopulateServices(_serviceCollection);
            
            RegisterAppContainerAsService();
            RegisterPluginModuleServices();
            RegisterDefaultServices();
            
            CreateCompositeLogger();
        }
        
        private void RegisterAppContainerAsService()
        {
            _serviceCollection.AddSingleton<ICompositeContainer>(this);
        }
        
        private void RegisterPluginModuleServices()
        {
            var modulesWithServices = _compositeApp.AllModules.OfType<IPluginModuleService>();

            foreach (IPluginModuleService moduleService in modulesWithServices)
            {
                var moduleServiceType = moduleService.GetType();
                var moduleServiceInterfaces = moduleServiceType.GetInterfacesDerivedFrom<IPluginModuleService>();

                _serviceCollection.AddSingleton(moduleServiceInterfaces, moduleService);
            }
        }

        private void RegisterDefaultServices()
        {
            _serviceCollection.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();
        }
        
        // The last step in the bootstrap process allowing plug-in modules to start runtime services.
        void IBuiltContainer.Start()
        {
            CreateServiceProvider();
            
            if (_compositeApp.IsStarted)
            {
                throw LogException(new ContainerException(
                    "The application container has already been started."));
            }

            try
            {
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapStart, "Starting Container"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _compositeApp.StartPluginModules(scope.ServiceProvider);
                }
               
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTraceDetails(BootstrapLogEvents.BootstrapCompositeLog, "Composite Log", Log);
                }
            }
            catch (ContainerException ex)
            {
                LogException(ex);
                throw;
            }
            catch (Exception ex)
            {
                throw LogException(new ContainerException(
                    "Error starting container. See Inner Exception.", ex));
            }
        }

        public void Stop()
        {
            if (! _compositeApp.IsStarted)
            {
                throw LogException(new ContainerException(
                    "The application container has not been started."));
            }

            try
            {
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapStop, "Stopping Container"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _compositeApp.StopPluginModules(scope.ServiceProvider);
                }
            }
            catch (ContainerException ex)
            {
                LogException(ex);
                throw;
            }
            catch (Exception ex)
            {
                throw LogException(new ContainerException(
                    "Error stopping container.  See Inner Exception.", ex));
            }
        }
        
        //-------------------------------- Container Services -----------------------------------------//
        
        private void ConfigureValidation()
        {
            _validationConfig = new ValidationConfig();
            
/*            _validationConfig = _configs.Values.OfType<ValidationConfig>()
                .FirstOrDefault() ?? new ValidationConfig();*/
        }
        
        // Creates a validation instance, based on the application configuration, used to validate an object.
        // The host application can specify an implementation using a validation library of choice.
        public IObjectValidator CreateValidator(object obj)
        {
            ThrowIfDisposed(this);
            return (IObjectValidator)Activator.CreateInstance(_validationConfig.ValidatorType, obj);
        }
        
        public T GetConfig<T>() where T : IPluginConfig
        {
            var config = _plugins.SelectMany(p => p.Configs).FirstOrDefault(
                pc => pc.GetType() == typeof(T));

            if (config == null)
            {
               // config = new T();
               // _configs.Add(config);  //TODO THROW
            }

            return (T)config;
        }

        //-------------------------------- Service Provider -------------------------------------------//
        
        // Returns a new service scope that can be used to instantiate services.  After the scope 
        // is used, it should be disposed.   When running within a host such as ASP.NET, the service 
        // scope is automatically created and disposed.
        public IServiceScope CreateServiceScope()
        {
            ThrowIfDisposed(this);
            return _serviceProvider.CreateScope();
        }

        // Execute a delegate within a new service scope instance that is disposed.
        public void ExecuteInServiceScope(Action<IServiceProvider> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            ThrowIfDisposed(this);

            using (var scope = CreateServiceScope())
            {
                action(scope.ServiceProvider);
            }
        }
        
        //------------------------------- Container Disposal ------------------------------------------//
       
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ThrowIfDisposed(CompositeContainer container)
        {
            if (container._disposed)
            {
                throw new ContainerException(
                    "The application container has been disposed and can no longer be accessed.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (! disposing || _disposed) return;

            if (_compositeApp.IsStarted)
            {
                Stop();
            }

            DisposePluginModules();

            _loggerFactory?.Dispose();
            _disposed = true;
        }

        private void DisposePluginModules()
        {
            foreach (var module in _compositeApp.AllPlugins.SelectMany(p => p.Modules))
            {
                (module as IDisposable)?.Dispose();
            }
        }

        //-------------------------------- Container Logging -------------------------------------//

        private Exception LogException(Exception ex)
        {
            _logger.LogErrorDetails(BootstrapLogEvents.BootstrapException, ex, "Bootstrap Exception");
            return ex;
        }

        private void LogPlugins(IEnumerable<IPlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                _logger.LogTraceDetails(BootstrapLogEvents.BootstrapPluginDetails, "Plug-in", new
                {
                    plugin.Name,
                    plugin.PluginId,
                    plugin.AssemblyName,
                   // Configs = plugin.PluginConfigs.Select(c => c.GetType().FullName),
                    Modules = plugin.Modules.Select(m => m.GetType().FullName)
                });
            }
        }
        
        private void CreateCompositeLogger()
        {
            _compositeLog = new CompositeAppLog(_compositeApp, _serviceCollection);
        }
    }
}