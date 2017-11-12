using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Configuration;
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
    /// After the application container is created, services registered by the plug-in 
    /// modules, can be accessed using the configured dependency injection container. 
    /// </summary>
    public class AppContainer : IAppContainer,
        IComposite,
        IBuiltContainer
    {
        // Singleton instance of created container.
        private static AppContainer _instance;
        private bool _disposed = false;

        private readonly ITypeResolver _typeResover;
        private readonly Dictionary<Type, IContainerConfig> _configs;  // ConfigType => Instance

        // Logging:
        private LoggerConfig _loggerConfig;
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;
        private CompositeLog _compositeLog;

        // Validation:
        private ValidationConfig _validationConfig;

        // Settings:
        private EnvironmentConfig _enviromentConfig;
        private IConfiguration _configuration;

        // Contains references to all the discovered plug-in manifests
        // used to bootstrap the container.
        private ManifestRegistry Registry { get; }

        private readonly CompositeApplication _application;
        private Autofac.IContainer _container;

        /// <summary>
        /// Creates an instance of the application container.  The static Create methods of AppContainer 
        /// are the suggested methods for creating a new container.  This constructor is usually used for 
        /// creating an application container for testing purposes.
        /// </summary>
        /// <param name="typeResolver">The type resolver implementation used to determine the plug-in
        /// components and their types.</param>
        /// <param name="setGlobalReference">Determines if AppContainer.Instance should be set to a
        /// singleton instance of the created container.  Useful for unit testing.</param>
        public AppContainer(ITypeResolver typeResolver, bool setGlobalReference = true)
        {
            _typeResover = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver),
                "Type Resolver implementation not specified.");

            _application = new CompositeApplication();
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

        // Creates a validation instance, based on the application configuration, used to validate an object.
        // The host application can specify an implementation using a validation library of choice.
        public IObjectValidator CreateValidator(object obj)
        {
            ThrowIfDisposed(this);
            return (IObjectValidator)Activator.CreateInstance(_validationConfig.ValidatorType, obj);
        }

        // The created dependency-injection container.
        public IContainer Services
        {
            get
            {
                ThrowIfDisposed(this);
                return _container;
            }
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

        CompositeApplication IComposite.Application
        {
            get { return _application; }
        }

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

        /// <summary>
        /// Creates an application container using the types provided by the specified type resolver.
        /// </summary>
        /// <param name="typeResolver">Reference to a custom type resolver.</param>
        /// <returns>Configured application container.</returns>
        public static IAppContainer Create(ITypeResolver typeResolver)
        {
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver),
                "Type Resolver implementation not specified.");

            if (_instance != null)
            {
                throw new ContainerException("Container has already been created.");
            }

            return new AppContainer(typeResolver);
        }

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
                "Configuration Initialization function not specified.");

            T config = new T();
            WithConfig(config);

            configInit(config);
            return this;
        }

        //------------------------------------------Container Build and Life Cycle-------------------------------------//

        // Loads and initializes all of the plug-ins and builds the DI container
        // but does not start their execution.
        public IBuiltContainer Build()
        {
            ConfigureEnvironment();
            ConfigureLogging();
            ConfigureValidation();
            LogContainerInitialization();

            try
            {
                using (var logger = _logger.LogTraceDuration(BootstrapLogEvents.BOOTSTRAP_BUILD, "Building Container"))
                {
                    LoadContainer();
                    ComposeLoadedPlugins();
                    SetKnownTypeDiscoveries();

                    LogPlugins(_application.Plugins);

                    CreateAutofacContainer();
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
        public void Start()
        {
            if (_application.IsStarted)
            {
                throw LogException(new ContainerException(
                    "The application container plug-in modules have already been started."));
            }

            try
            {
                using (var logger = _logger.LogTraceDuration(BootstrapLogEvents.BOOTSTRAP_START, "Starting Container"))
                {
                    _application.StartPluginModules(_container);
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
            if (!_application.IsStarted)
            {
                throw LogException(new ContainerException(
                    "The application container plug-in modules have not been started."));
            }

            try
            {
                using (var logger = _logger.LogTraceDuration(BootstrapLogEvents.BOOTSTRAP_STOP, "Stopping Container"))
                {
                    _application.StopPluginModules(_container);
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
            if (!disposing || _disposed) return;

            if (_application.IsStarted)
            {
                Stop();
            }

            DisposePluginModules();

            _container?.Dispose();
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

        // Configure overall environment settings such as .NET configuration extensions.
        private void ConfigureEnvironment()
        {
            _enviromentConfig = _configs.Values.OfType<EnvironmentConfig>()
                .FirstOrDefault() ?? new EnvironmentConfig();

            _configuration = _enviromentConfig.Configuration;
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

        // Delegate to the type resolver to search all assemblies representing plug-ins.
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
            _typeResover.SetPluginResolvedTypes(plugin);

            // Assign all configurations that are instances of types defined within plug-in.
            plugin.PluginConfigs = plugin.CreatedFrom(_configs.Values).ToList();
        }

        // This allows the plug-in to find concrete types deriving from IKnownPluginType.
        // This is how plug-in *modules* are composed.  All plug-in *module* properties 
        // that are of type: IEnumerable<T> where T is a derived IKnownPluginType will be 
        // set to instances of types deriving from T.
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
            var pluginDiscoveredTypes = new HashSet<Type>();
            foreach (IPluginModule module in plugin.Modules)
            {
                IEnumerable<Type> discoveredTypes = _typeResover.SetPluginModuleKnownTypes(module, fromPluginTypes);
                discoveredTypes.ForEach(dt => pluginDiscoveredTypes.Add(dt));
            }

            // Record all the types discovered by the plug-in.  Only used for logging.
            plugin.DiscoveredTypes = pluginDiscoveredTypes.ToArray();
        }

        private void SetKnownTypeDiscoveries()
        {
            _application.Plugins.ForEach(SetDiscoveredKnowTypes);
        }

        // For plug-in derived known-type, find the plug-in(s) that discovered the type.  
        // This information is used for logging how the application was composed.
        private void SetDiscoveredKnowTypes(Plugin plugin)
        {
            foreach (PluginType knownType in plugin.PluginTypes.Where(pt => pt.IsKnownType))
            {
                knownType.DiscoveredByPlugins = _application.Plugins
                    .Where(p => p.DiscoveredTypes.Any(dt => knownType.Type.IsConcreteTypeDerivedFrom(dt)))
                    .ToArray();
            }
        }

        //------------------------------------------DI Container Build------------------------------------------//

        private void CreateAutofacContainer()
        {
            var builder = new Autofac.ContainerBuilder();

            // Allow the composite application plug-in modules
            // to register services with container.
            _application.RegisterComponents(builder);

            // Register additional services,
            RegisterAppContainerAsService(builder);
            RegisterPluginModuleServices(builder);
            RegisterHostProvidedServices(builder);
            RegisterContainerProvidedServices(builder);

            // Register logging and configuration.
            RegisterLogging(builder);
            RegisterConfigSettings(builder);

            _container = builder.Build();
        }

        private void RegisterAppContainerAsService(Autofac.ContainerBuilder builder)
        {
            builder.RegisterInstance(this).As<IAppContainer>().SingleInstance();
        }

        // Register MS Logging Extensions.
        private void RegisterLogging(Autofac.ContainerBuilder builder)
        {
            builder.RegisterInstance(LoggerFactory).As<ILoggerFactory>().SingleInstance();
            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>)).SingleInstance();
        }

        // Register MS Configuration Extensions.
        private void RegisterConfigSettings(Autofac.ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(OptionsManager<>)).As(typeof(IOptions<>)).SingleInstance();
            builder.RegisterGeneric(typeof(OptionsMonitor<>)).As(typeof(IOptionsMonitor<>)).SingleInstance();
            builder.RegisterGeneric(typeof(OptionsSnapshot<>)).As(typeof(IOptionsSnapshot<>)).InstancePerLifetimeScope();
            builder.RegisterInstance(_configuration).As<IConfiguration>();
        }

        private void RegisterPluginModuleServices(Autofac.ContainerBuilder builder)
        {
            var modulesWithServices = _application.AllPluginModules.OfType<IPluginModuleService>();

            foreach (IPluginModuleService moduleService in modulesWithServices)
            {
                var moduleServiceType = moduleService.GetType();
                var moduleServiceInterfaces = moduleServiceType.GetInterfacesDerivedFrom<IPluginModuleService>();

                builder.RegisterInstance(moduleService)
                    .As(moduleServiceInterfaces.ToArray())
                    .SingleInstance();
            }
        }

        // Allow the host application to register any service types or instances created during 
        // the initialization of the application.
        private void RegisterHostProvidedServices(Autofac.ContainerBuilder builder)
        {
            var regConfig = _configs.Values.OfType<AutofacRegistrationConfig>().FirstOrDefault();

            if (regConfig != null && regConfig.Build != null)
            {
                regConfig.Build(builder);
            }
        }

        private void RegisterContainerProvidedServices(Autofac.ContainerBuilder builder)
        {
            builder.RegisterType<ValidationService>()
                .As<IValidationService>()
                .SingleInstance();
        }

        //------------------------------------------Logging------------------------------------------//

        // Determines if the host application specified how logging should
        // be configured.  If not specified, a default configuration is used.
        private void ConfigureLogging()
        {
            _loggerConfig = _configs.Values.OfType<LoggerConfig>()
               .FirstOrDefault() ?? new LoggerConfig();

            _loggerFactory = _loggerConfig.LoggerFactory;

            // Since no longer specified, all logs having level greater than Warning will
            // be written to standard debug output.
            if (_loggerFactory == null)
            {
                _loggerFactory = new LoggerFactory();
                _loggerFactory.AddDebug(LogLevel.Warning);
                _loggerConfig.LogExceptions = true;
            }

            // Create logger to be used by this class and also provide the composite application
            // with a reference to the log factory so it can create plug-in-specific loggers.
            _logger = _loggerFactory.CreateLogger<AppContainer>();
            _application.LoggerFactory = _loggerFactory;
        }

        private Exception LogException(Exception ex)
        {
            if (_loggerConfig.LogExceptions)
            {
                _logger.LogError(BootstrapLogEvents.BOOTSTRAP_EXCEPTION, "Bootstrap Exception", ex);
            }
            return ex;
        }

        private void CreateCompositeLogger()
        {
            _compositeLog = new CompositeLog(_application, _container.ComponentRegistry);
        }

        private void LogContainerInitialization()
        {
            _logger.LogDebugDetails(BootstrapLogEvents.BOOTSTRAP_INITIALIZE, "Container Setup", new
            {
                TypeResolver = _typeResover.GetType().AssemblyQualifiedName,
                Configs = _configs.Keys.Select(ct => ct.AssemblyQualifiedName)
            });
        }

        private void LogManifests(ManifestRegistry registry)
        {
            _logger.LogDebugDetails(BootstrapLogEvents.BOOTSTRAP_EXCEPTION, "Manifests", new
            {
                Host = registry.AppHostPluginManifests.First().GetType().AssemblyQualifiedName,
                Application = registry.AppComponentPluginManifests.Select(m => m.GetType().AssemblyQualifiedName),
                Core = registry.CorePluginManifests.Select(c => c.GetType().AssemblyQualifiedName)
            });
        }

        private void LogPlugins(Plugin[] plugins)
        {
            foreach (var plugin in plugins)
            {
                _logger.LogTraceDetails(BootstrapLogEvents.BOOTSTRAP_PLUGIN_DETAILS, "Plug-in", new
                {
                    plugin.Manifest.Name,
                    plugin.Manifest.PluginId,
                    plugin.AssemblyName,
                    Configs = plugin.PluginConfigs.Select(c => c.GetType().FullName),
                    Modules = plugin.Modules.Select(m => m.GetType().FullName),
                    Discovers = plugin.DiscoveredTypes.Select(t => t.FullName)
                });
            }
        }
    }
}