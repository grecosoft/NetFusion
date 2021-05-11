using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base;
using NetFusion.Base.Logging;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Resulting Composite-Application built from a set of plugins.  A reference to a singleton instance
    /// of this class can be referenced by injecting ICompositeApp into a dependent service component.
    /// If the dependent component is not registered within the container, the Composite-Application can
    /// be accessed using the CompositeApp.Instance property.
    /// </summary>
    public class CompositeApp : ICompositeApp
    {
        public PluginSummary HostPlugin { get; }

        // Singleton instance of the composite-application:
        public static ICompositeApp Instance { get; private set; }
        public bool IsStarted { get; private set; }
        
        // Reference to the builder details that constructed the composite-application.
        private readonly ICompositeAppBuilder _builder;
        
        // The service-provider associated with the application created from the
        // service-collection populated by the bootstrapped plugin modules.
        private readonly IServiceProvider _serviceProvider;
        
        private readonly ILogger _logger;
        
        public CompositeApp(
            ICompositeAppBuilder builder,
            IServiceProvider serviceProvider,
            ILogger<CompositeApp> logger)
        {
            Instance = this;
            IsStarted = false;
            
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            HostPlugin = new PluginSummary(builder.HostPlugin);
        }

        // --------------------------- [Starting Composite Application] -----------------------------------
        
        // At this point, the composite-application has been created from all the registered plugins and
        // all modules have been initialized and services registered.  This is the last call allowing each
        // plugin module to execute code within the created dependency-injection container before the host
        // application is started.
        public async Task StartAsync()
        {
            const string startExMsg = "Error Starting Composite Application";
            
            if (IsStarted)
            {
                NfExtensions.Logger.Log<CompositeApp>(LogLevel.Error, "Composite Application already started");
                throw new ContainerException("Composite Application already started");
            }

            try
            {
                // Create a service scope in which each plugin can be started:
                using (_logger.LogInformationDuration("Starting Modules"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {                    
                    NfExtensions.Logger.Log<CompositeApp>(CoreServicesLogger.Log(scope.ServiceProvider));
                    await StartModules(scope.ServiceProvider);
                }
            }
            catch (ContainerException ex)
            {
                NfExtensions.Logger.LogError<CompositeApp>(ex, startExMsg);
                throw;
            }
            catch (AggregateException ex)
            {
                var flattenedEx = ex.Flatten();
                
                NfExtensions.Logger.LogError<CompositeApp>(flattenedEx, startExMsg);
                throw new ContainerException(startExMsg, flattenedEx);
                
            }
            catch (Exception ex)
            {
                NfExtensions.Logger.LogError<CompositeApp>(ex, startExMsg);
                throw new ContainerException(startExMsg, ex);
            }
        }

        public void Start()
        {
            StartAsync().GetAwaiter().GetResult();
        }
        
        private async Task StartModules(IServiceProvider services)
        {
            // Start the plug-in modules in dependent order starting with core modules 
            // and ending with the application host modules.
            IsStarted = true;

            // Now that the service-provider has been created, initialize each
            // module so it can use ILoggerFactory to obtain a logger.
            foreach (IPluginModule module in _builder.AllModules)
            {
                module.Context.InitLogging(services);
            }

            var startTasks = StartPlugins(services);
            await Task.WhenAll(startTasks);
            
            _logger.LogInformation("All Modules Started");

            // Last phase to allow any modules to execute any processing that
            // might be dependent on another module being started.
            var runTasks = RunPlugins(services);
            await Task.WhenAll(runTasks);
            
            _logger.LogInformation("All Modules Ran");
        }

        private Task[] StartPlugins(IServiceProvider services)
        {
            var coreStartTask = StartPluginModules(services, _builder.CorePlugins);
            var appStartTask = StartPluginModules(services, _builder.AppPlugins);
            var hostStartTask = StartPluginModules(services, _builder.HostPlugin);

            return new[] {coreStartTask, appStartTask, hostStartTask};
        }

        private async Task StartPluginModules(IServiceProvider services, params IPlugin[] plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                foreach (IPluginModule module in plugin.Modules)
                {
                    _logger.LogDebug("Starting Module: {moduleType} for Plugin: {pluginName}", 
                        module.GetType().Name, 
                        module.Context.Plugin.Name);
                
                    await module.StartModuleAsync(services);
                }
            }
        }

        private Task[] RunPlugins(IServiceProvider services)
        {
            var coreRunTask = RunPluginModules(services, _builder.CorePlugins);
            var appRunTask = RunPluginModules(services, _builder.AppPlugins);
            var hostRunTask = RunPluginModules(services, _builder.HostPlugin);

            return new[] {coreRunTask, appRunTask, hostRunTask};
        }

        private async Task RunPluginModules(IServiceProvider services, params IPlugin[] plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                foreach (IPluginModule module in plugin.Modules)
                {
                    _logger.LogDebug("Running Module: {moduleType} for Plugin: {pluginName}", 
                        module.GetType().Name, 
                        module.Context.Plugin.Name);
                
                    await module.RunModuleAsync(services);
                }
            }
        }

        // --------------------------- [Runtime Services] -------------------------------
        
        public IDictionary<string, object> Log
        {
            get
            {
                ThrowIfStopped();
                return _builder.CompositeLog?.GetLog() ?? new Dictionary<string, object>();
            }
        }
        
        // Returns a new service scope that can be used to instantiate services.  After the scope 
        // is used, it should be disposed.   When running within a host such as ASP.NET, the service 
        // scope is automatically created and disposed.
        public IServiceScope CreateServiceScope()
        {
            ThrowIfStopped();
            return _serviceProvider.CreateScope();
        }

        // --------------------------- [Stopping Composite Application] -------------------------------
        
        // Should be called when the application-host is stopped.  Each registered
        // plugin-module is stopped allowing it to reclaim resources.
        public async Task StopAsync()
        {
            const string stopExMsg = "Error Stopping Composite Application";
            
            if (! IsStarted)
            {
                return;
            }
            
            try
            {
                _logger.LogInformation("Composite Application Stopping.");
                
                // Create a service scope in which each plugin can be stopped:
                using (_logger.LogInformationDuration("Stopping Composite-Application"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    await StopPluginModulesAsync(scope.ServiceProvider);
                }
            }
            catch (ContainerException ex)
            {
                NfExtensions.Logger.LogError<CompositeApp>(ex, stopExMsg);
                throw;
            }
            catch (AggregateException ex)
            {
                var flattenedEx = ex.Flatten();
                
                NfExtensions.Logger.LogError<CompositeApp>(flattenedEx, stopExMsg);
                throw new ContainerException(stopExMsg, flattenedEx);
            }
            catch (Exception ex)
            {
                NfExtensions.Logger.LogError<CompositeApp>(ex, stopExMsg);
                throw new ContainerException(stopExMsg, ex);
            }
        }
        
        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }
        
        private async Task StopPluginModulesAsync(IServiceProvider services)
        {
            IsStarted = false;
            
            var hostStopTask = StopPluginModules(services, _builder.HostPlugin);
            var appStopTask = StopPluginModules(services, _builder.AppPlugins);
            var coreStopTask = StopPluginModules(services, _builder.CorePlugins);

            await Task.WhenAll(hostStopTask, appStopTask, coreStopTask);
        }

        private async Task StopPluginModules(IServiceProvider services, params IPlugin[] plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                var pluginModules = plugin.Modules.Reverse().ToArray();
                foreach (IPluginModule module in pluginModules)
                {
                    _logger.LogDebug("Stopping Module: {moduleType} for Plugin: {pluginName}", 
                        module.GetType().Name, 
                        module.Context.Plugin.Name);
                
                    await module.StopModuleAsync(services);
                }
            }
        }

        private void ThrowIfStopped()
        {
            if (! IsStarted)
            {
                throw new ContainerException("Stopped Composite Application can no longer be accessed");
            }
        }
    }
}