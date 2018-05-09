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
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Responsible for bootstrapping an application composed from plug-in components.
    /// The bootstrap orchestrates the construction of the application container and
    /// the service collection that is populated.  An assembly is identified as being
    /// a plug-in by containing a class deriving from one of the base IPluginManifest
    /// types used to identify the category of plug-in:
    /// 
    ///     - AppHost:  The executable process (WebApi/Console)
    ///     - AppComponent:  Contains application specific components.
    ///     - Core: Contains technology centric reusable implementations and cross-cut 
    ///     concerns.
    ///     
    /// The application container and bootstrap process is only dependent on Microsoft
    /// libraries.  Core plug-ins should be written for non  Microsoft open-source
    /// implementations.  This allows the base implementation to have a small footprint
    /// that can be easily extended without requiring changes.
    /// </summary>
    public class AppContainer : IAppContainer,
        IComposite,
        IBuiltContainer
    {
        // Singleton instance of created container:
        private static AppContainer _instance;
        private bool _disposed = false;

        // Microsoft Common Abstractions:
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        
        // Used abstraction implementations:
        private readonly ILogger _logger;
        private IServiceProvider _serviceProvider;

        // Composite Application:
        private readonly ITypeResolver _typeResover;
        private ManifestRegistry Registry { get; }
        private readonly Dictionary<Type, IContainerConfig> _configs;  // ConfigType => Instance
        private readonly CompositeApplication _application;
        private CompositeLog _compositeLog;

        // Container Abstractions:
        private ValidationConfig _validationConfig;
        
        /// <summary>
        /// Creates an instance of the application container.  An instance of this class should be
        /// created using the ContainerBuilder class during startup of the application host.
        /// </summary>
        /// <param name="services">Abstraction of a collection of services populated by plug-in modules.</param>
        /// <param name="configuration">Abstraction used to read application configuration settings.</param>
        /// <param name="loggerFactory">Abstraction used to log messages.</param>
        /// <param name="typeResolver">Abstraction used to discover plug-ins and their types.</param>
        /// <param name="setGlobalReference">Determines if AppContainer.Instance should be set to a
        /// singleton instance of the created container.  Useful for unit testing where a shared 
        /// instance my not be wanted.</param>
        public AppContainer(
            IServiceCollection services, 
            IConfiguration configuration,
            ILoggerFactory loggerFactory, 
            ITypeResolver typeResolver, 
            bool setGlobalReference = true)
        {
            // Dependent Core Abstraction Implementations:
            _serviceCollection = services ?? throw new ArgumentNullException(nameof(services));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _typeResover = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
                  
            // Composite Application:
            _application = new CompositeApplication(configuration, loggerFactory);
            _logger = _loggerFactory.CreateLogger<AppContainer>();
            _configs = new Dictionary<Type, IContainerConfig>();  

            if (setGlobalReference)
            {
                _instance = this;
            }
            
            Registry = new ManifestRegistry();
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
        /// be used when service-locater is necessary from a root component.  If a given
        /// service implementation is contained within the service collection, it can 
        /// reference the application container by injecting the IAppContainer interface.
        /// </summary>
        public static IAppContainer Instance
        {
            get
            {
                ThrowIfDisposed(_instance);
                return _instance;
            }
        }

        public ILoggerFactory LoggerFactory
        {
            get
            {
                ThrowIfDisposed(this);
                return _loggerFactory;
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

        // Creates a validation instance, based on the application configuration, used to validate an object.
        // The host application can specify an implementation using a validation library of choice.
        public IObjectValidator CreateValidator(object obj)
        {
            ThrowIfDisposed(this);
            return (IObjectValidator)Activator.CreateInstance(_validationConfig.ValidatorType, obj);
        }

        private static void ThrowIfDisposed(AppContainer container)
        {
            if (container._disposed)
            {
                throw new ContainerException(
                    "The application container has been disposed and can no longer be accessed.");
            }
        }

        //------------------------------------------ IComposite Methods -----------------------------------------------//
        // This is an interface exposed for use by components that may need details about the composite application.
        // This interface is often used when unit-testing and not by typical business applications.

        CompositeApplication IComposite.Application => _application;

        Plugin IComposite.GetPluginContainingType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type), 
                "Reference to type not specified.");

            return _application.Plugins.FirstOrDefault(p => p.PluginTypes.Any(pt => pt.Type == type));
        }

        Plugin IComposite.GetPluginContainingFullTypeName(string fullTypeName)
        {
            if (string.IsNullOrWhiteSpace(fullTypeName))
                throw new ArgumentException("Full type not cannot be null or whitespace.", nameof(fullTypeName));

            return _application.Plugins.FirstOrDefault(p => p.PluginTypes.Any(pt => pt.Type.FullName == fullTypeName));
        }

        //---------------------------------------Container Creation-------------------------------//

        public IAppContainer WithConfig(IContainerConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config), 
                "Configuration cannot be null.");
            
            if (Registry.AllManifests != null)
            {
                throw new ContainerException("Container has already been built.");
            }

            var configType = config.GetType();
            if (_configs.ContainsKey(configType))
            {
                throw new ContainerException(
                    $"Existing configuration of type: {config.GetType()} is already configured.");
            }

            _configs[configType] = config;
            return this;
        }

        public IAppContainer WithConfig<T>()
            where T : IContainerConfig, new()
        {
            WithConfig(new T());
            return this;
        }

        public IAppContainer WithConfig<T>(Action<T> configInit)
            where T : IContainerConfig, new()
        {
            if (configInit == null) throw new ArgumentNullException(nameof(configInit), 
                "Configuration delegate not specified.");

            T config = new T();
            WithConfig(config);

            configInit(config);
            return this;
        }

        //------------------------------------------Container Build and Life Cycle-------------------------------------//

        // Loads and initializes all of the plug-ins and builds the service collection but does not start their execution.
        public IBuiltContainer Build()
        {
            ConfigureValidation();

            try
            {
                using (_logger.LogTraceDuration(BootstrapLogEvents.BOOTSTRAP_BUILD, "Building Container"))
                {
                    LoadContainer();
                    ComposeLoadedPlugins();
                    SetDiscoveredTypes();

                    LogPlugins(_application.Plugins);

                    CreateServiceProvider();
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

        // The last step in the bootstrap process allowing plug-in modules to start runtime services.
        void IBuiltContainer.Start()
        {
            if (_application.IsStarted)
            {
                throw LogException(new ContainerException(
                    "The application container has already been started."));
            }

            try
            {
                using (_logger.LogTraceDuration(BootstrapLogEvents.BOOTSTRAP_START, "Starting Container"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _application.StartPluginModules(scope.ServiceProvider);
                }
               
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTraceDetails(BootstrapLogEvents.BOOTSTRAP_COMPOSITE_LOG, "Composite Log", Log);
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
            if (! _application.IsStarted)
            {
                throw LogException(new ContainerException(
                    "The application container has not been started."));
            }

            try
            {
                using (_logger.LogTraceDuration(BootstrapLogEvents.BOOTSTRAP_STOP, "Stopping Container"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _application.StopPluginModules(scope.ServiceProvider);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (! disposing || _disposed) return;

            if (_application.IsStarted)
            {
                Stop();
            }

            DisposePluginModules();

            _loggerFactory?.Dispose();
            _disposed = true;
        }

        private void DisposePluginModules()
        {
            foreach (var module in _application.AllPluginModules)
            {
                (module as IDisposable)?.Dispose();
            }
        }

        private void ConfigureValidation()
        {
            _validationConfig = _configs.Values.OfType<ValidationConfig>()
                .FirstOrDefault() ?? new ValidationConfig();
        }

        private void LoadContainer()
        {
            LoadManifestRegistry();
            LoadPlugins();
        }


        //------------------------------------------Plug-in Loading-------------------------------------//

        // Delegate to the type resolver to search for all assemblies representing plug-ins.
        private void LoadManifestRegistry()
        {
            var validator = new ManifestValidation(Registry);

            _typeResover.Initialize(_loggerFactory);
            _typeResover.SetPluginManifests(Registry);
            validator.Validate();

            LogManifests(Registry);
        }

        // For each found plug-in manifest assembly, create a plug-in instance
        // associated with the manifest and add to composite application.  Then
        // load all plug-in instances.
        private void LoadPlugins()
        {
            _application.Plugins = Registry.AllManifests
                .Select(m => new Plugin(m))
                .ToArray();

            _application.Plugins.ForEach(LoadPlugin);
        }

        private void LoadPlugin(Plugin plugin)
        {
            // Locates any IPluginModule instances defined within the plug-in assembly.
            _typeResover.SetPluginResolvedTypes(plugin);

            // Assign all configuration instances of configuration types defined within plug-in.
            plugin.PluginConfigs = plugin.CreatedFrom(_configs.Values).ToList();
        }

        // This allows the plug-in to find concrete types deriving from IKnownPluginType.
        // All plug-in *module* properties that are of type: IEnumerable<T> where T is a 
        // derived IKnownPluginType will be set to instances of types deriving from T.
        // NOTE:  think of this as a very light version of Managed Extensibility Framework
        // that does only what we need and does not try to solve World problems :)
        private void ComposeLoadedPlugins()
        {
            ComposeCorePlugins();
            ComposeAppPlugins();
        }

        // Core plug-in modules discover derived known-types contained within *all* plug-ins since they
        // are core and provide cross-cutting features to other core and application level plug-ins.
        private void ComposeCorePlugins()
        {
            var allPluginTypes = _application.GetPluginTypes();

            _application.CorePlugins.ForEach(p =>
                ComposePluginModules(p, allPluginTypes));
        }

        // Application plug-in modules search for derived known-types contained *only* within other 
        // application plug-ins.  Core plug in types are not included since application plug-ins
        // never provide functionality to lower level plug-ins.
        private void ComposeAppPlugins()
        {
            var allAppPluginTypes = _application.GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);

            _application.AppComponentPlugins.ForEach(p =>
                ComposePluginModules(p, allAppPluginTypes));

            ComposePluginModules(_application.AppHostPlugin, allAppPluginTypes);
        }

        private void ComposePluginModules(Plugin plugin, IEnumerable<PluginType> fromPluginTypes)
        {
            foreach (IPluginModule module in plugin.Modules)
            {
                _typeResover.SetPluginModuleKnownTypes(module, fromPluginTypes);
            }
        }

        private void SetDiscoveredTypes()
        {
            _application.Plugins.ForEach(SetDiscoveredTypes);
        }

        // For each plug-in concrete derived known-type, find the plug-in(s) defining the base types 
        // on which it is based.  This information is used for logging how the application was composed.
        private void SetDiscoveredTypes(Plugin plugin)
        {
            PluginType[] thisPluginKnowTypeImplementations = plugin.PluginTypes.Where(pt => pt.IsKnownTypeImplementation).ToArray();

            PluginType[] allPluginKnownTypeContracts = _application.Plugins
                .SelectMany(p => p.PluginTypes)
                .Where(pt => pt.IsKnownTypeContract)
                .ToArray();

            foreach (PluginType thisPluginKnowType in thisPluginKnowTypeImplementations)
            {
                thisPluginKnowType.DiscoveredByPlugins = allPluginKnownTypeContracts.Where(
                        kt => thisPluginKnowType.Type.IsConcreteTypeDerivedFrom(kt.Type))
                    .Select(kt => kt.Plugin)
                    .Distinct();
            }
        }

        //------------------------------------------DI Container Build------------------------------------------//

        private void CreateServiceProvider()
        {          
            // Allow the composite application plug-in modules to register services.
            _application.PopulateServices(_serviceCollection);

            // Register additional services,
            RegisterAppContainerAsService();
            RegisterPluginModuleServices();
            RegisterContainerProvidedServices();
  
            // Create service provider from populated service collection.
            _serviceProvider = _serviceCollection.BuildServiceProvider(true);
        }

        private void RegisterAppContainerAsService()
        {
            _serviceCollection.AddSingleton<IAppContainer>(this);
        }

        private void RegisterPluginModuleServices()
        {
            var modulesWithServices = _application.AllPluginModules.OfType<IPluginModuleService>();

            foreach (IPluginModuleService moduleService in modulesWithServices)
            {
                var moduleServiceType = moduleService.GetType();
                var moduleServiceInterfaces = moduleServiceType.GetInterfacesDerivedFrom<IPluginModuleService>();

                _serviceCollection.AddSingleton(moduleServiceInterfaces, moduleService);
            }
        }

        private void RegisterContainerProvidedServices()
        {
            _serviceCollection.AddSingleton<IConfiguration>(_configuration);
            _serviceCollection.AddSingleton<IValidationService, ValidationService>();
            _serviceCollection.AddSingleton<IEntityScriptingService, NullEntityScriptingService>();            
        }

        //------------------------------------------Logging------------------------------------------//
        private Exception LogException(Exception ex)
        {
            _logger.LogErrorDetails(BootstrapLogEvents.BOOTSTRAP_EXCEPTION, ex, "Bootstrap Exception");
            return ex;
        }

        private void CreateCompositeLogger()
        {
            _compositeLog = new CompositeLog(_application, _serviceCollection);
        }

        private void LogManifests(ManifestRegistry registry)
        {
            _logger.LogDebugDetails(BootstrapLogEvents.BOOTSTRAP_EXCEPTION, "Manifests", new
            {
                TypeResolver = _typeResover.GetType().AssemblyQualifiedName,
                Host = registry.AppHostPluginManifests.First().GetType().AssemblyQualifiedName,
                Application = registry.AppComponentPluginManifests.Select(m => m.GetType().AssemblyQualifiedName),
                Core = registry.CorePluginManifests.Select(c => c.GetType().AssemblyQualifiedName)
            });
        }

        private void LogPlugins(IEnumerable<Plugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                _logger.LogTraceDetails(BootstrapLogEvents.BOOTSTRAP_PLUGIN_DETAILS, "Plug-in", new
                {
                    plugin.Manifest.Name,
                    plugin.Manifest.PluginId,
                    plugin.AssemblyName,
                    Configs = plugin.PluginConfigs.Select(c => c.GetType().FullName),
                    Modules = plugin.Modules.Select(m => m.GetType().FullName)
                });
            }
        }
    }
}