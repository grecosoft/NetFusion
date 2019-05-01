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
        // Singleton instance of the created container:
        private static CompositeContainer _instance;
        private bool _disposed;

        // Microsoft Common Abstractions:
        private readonly IServiceCollection _serviceCollection;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
       
        // Logging implementations:
        private readonly ILogger _logger;
        private CompositeAppLog _compositeLog;
       
        // Composite Structure:
        private CompositeApp _compositeApp;
        private readonly List<IPlugin> _plugins = new List<IPlugin>();
        private readonly List<IPluginConfig> _containerConfigs = new List<IPluginConfig>();
        
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

            AddContainerConfigs();
        }
        
        // Log of the composite application structure showing how it was constructed from plugins.
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
        /// reference the application container by injecting the ICompositeContainer interface.
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
        
        // Called by CompositeContainerBuilder to add plugin to the composite container.
        // If the plugin type is already registered, the request is ignored.  This allows
        // a plugin to register it's dependent plugins.
        internal void RegisterPlugin<T>() where T : IPlugin, new()
        {
            if (IsPluginRegistered<T>())
            {
                return;
            }
            
            IPlugin plugin = new T();
            _plugins.Add(plugin);
        }

        private bool IsPluginRegistered<T>() where T : IPlugin
        {
            return _plugins.Any(p => p.GetType() == typeof(T));
        }
        
        //----------------------- Container Configuration ------------------------------------//
        
        // Finds a configuration belonging to one of the registered plugins.  When a plugin
        // is registered with the container, it can extend the behavior of another plugin by
        // requesting a configuration from the other plugin and setting information used to
        // extended the base implementation.
        public T GetPluginConfig<T>() where T : IPluginConfig
        {
            var configs = _plugins.SelectMany(p => p.Configs)
                .Where(c => c.GetType() == typeof(T)).ToArray();

            if (configs.Length > 1)
            {
                throw LogException(new ContainerException(
                    $"Plugin configuration of type: {typeof(T)} has been registered by multiple plugins." +
                     "A configuration can be registered by only one plugin."));
            }

            if (configs.Length == 0)
            {
                throw LogException(new ContainerException(
                    $"Plugin configuration of type: {typeof(T)} is not registered."));
            }

            return (T)configs.First();
        }
        
        // Returns a container level configuration used to configure the runtime behavior
        // of the built container.
        internal T GetContainerConfig<T>() where T : IPluginConfig
        {
            var config = _containerConfigs.FirstOrDefault(c => c.GetType() == typeof(T));
            if (config == null)
            {
                throw LogException(new ContainerException(
                    $"Container configuration of type: {typeof(T)} is not registered."));
            }

            return (T)config;
        }
        
        private void AddContainerConfigs()
        {
            _containerConfigs.Add(new ValidationConfig());
        }
        
        //----------------------- Container Initialization ------------------------------------//
        
        // Composes the container for the registered plugins and populates the service-collection.
        internal IBuiltContainer Compose(ITypeResolver typeResolver)
        {
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            
            _compositeApp = new CompositeApp(_loggerFactory, _configuration, _plugins);
            
            try
            {
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapCompose, "Composing Container"))
                {
                    ResolvePlugins(typeResolver);
           
                    // Validates that the application was composed for a valid set of plugins.
                    _compositeApp.Validate();
                    
                    // Allow each plug-in module to compose itself from concrete types, defined
                    // by other plugins, based on abstract types it defines. 
                    ComposeCorePlugins(typeResolver);
                    ComposeApplicationPlugins(typeResolver);
                    
                    LogPlugins(_compositeApp.AllPlugins);

                    // With all the plugins composed allow each plugin to add its needed
                    // services to the service-collection.
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

        // Delegates to the type resolver to populate information and the types associated with each plugin.
        // This decouples the container from runtime information and makes it easier to test.
        private void ResolvePlugins(ITypeResolver typeResolver)
        {           
            foreach (IPlugin plugin in _compositeApp.AllPlugins)
            {
                typeResolver.SetPluginMeta(plugin);
            }
        }

        // Core plugins are composed from all other plugin types since they implement
        // reusable cross-cutting concerns.
        private void ComposeCorePlugins(ITypeResolver typeResolver)
        {
            var allPluginTypes = _compositeApp.GetPluginTypes().ToArray();

            foreach (var plugin in _compositeApp.CorePlugins)
            {
                typeResolver.ComposePlugin(plugin, allPluginTypes);
            }
        }

        // Application plugins contain a specific application's implementation
        // and are composed only from other application specific plugins.
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
            // Allows each plugin-module to add services to the collection.
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
            _serviceCollection.AddSingleton<IValidationService, ValidationService>();
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
        
        //-------------------------------- Container Provided Services -----------------------------------------//
       
        // Creates a validation instance, based on the application configuration, used to validate an object.
        // The host application can specify an implementation using a validation library of choice.
        public IObjectValidator CreateValidator(object obj)
        {
            ThrowIfDisposed(this);

            ValidationConfig validationConfig = GetContainerConfig<ValidationConfig>();
            return (IObjectValidator)Activator.CreateInstance(validationConfig.ValidatorType, obj);
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
                    Configs = plugin.Configs.Select(c => c.GetType().FullName),
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