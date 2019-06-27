using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Bootstrap.Validation;

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
        // Singleton instance of the composite-application:
        public static ICompositeApp Instance { get; private set; }
        
        private readonly ICompositeAppBuilder _builder;
        private readonly IServiceProvider _serviceProvider;
        private bool _isStarted;
        
        private readonly ILogger _logger;
        
        public CompositeApp(
            ICompositeAppBuilder builder,
            IServiceProvider serviceProvider,
            ILogger<CompositeApp> logger)
        {
            Instance = this;
            _isStarted = false;

            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

  
        //-----------------------------------------------------
        //--Starting Composite Application
        //-----------------------------------------------------
        
        // At this point, the composite-application has been created from all the 
        // registered plugins and all modules have been initialized and services
        // registered.  This is the last call allowing each plugin module to
        // execute code within the created dependency-injection container before
        // the host application is started.
        public async Task StartAsync()
        {
            if (_isStarted)
            {
                throw LogException(new ContainerException(
                    "The application container has already been started."));
            }

            // Write the logs that were recorded before the container-provider
            // was built now that ILogger is available.
            _builder.BootstrapLogger.WriteToLogger(_logger);

            // If there were any bootstrap errors,  raise an exception to 
            // abort starting the composite-application.
            if (_builder.BootstrapLogger.HasErrors)
            {
                throw new ContainerException(
                    "Errors were recorded when bootstrapping application.  See log for details.");
            }
            
            try
            {
                // Create a service scope in which each plugin can be started:
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapStart, "Starting Container"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    await StartModules(scope.ServiceProvider);
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
            catch (AggregateException ex)
            {
                var flattenedEx = ex.Flatten();
                throw LogException(new ContainerException(
                    "Error starting container.  See Inner Exception.", flattenedEx));
                
            }
            catch (Exception ex)
            {
                throw LogException(new ContainerException(
                    "Error starting container. See Inner Exception.", ex));
            }
        }

        public void Start()
        {
            StartAsync().GetAwaiter().GetResult();
        }
        
        private async Task StartModules(IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services),
                "Services cannot be null.");

            // Start the plug-in modules in dependent order starting with core modules 
            // and ending with the application host modules.
            _isStarted = true;

            // Allow each module context to be initialized with services only available
            // after the service-provider has been created (i.e. logging)
            foreach (IPluginModule module in _builder.AllModules)
            {
                module.Context.Initialize(services);
            }
     
            var coreStartTask = StartPluginModules(services, _builder.CorePlugins);
            var appStartTask = StartPluginModules(services, _builder.AppPlugins);
            var hostStartTask = StartPluginModules(services, new[] { _builder.HostPlugin });

            await Task.WhenAll(coreStartTask, appStartTask, hostStartTask);

            // Last phase to allow any modules to execute any processing that
            // might be dependent on another module being started.
            await RunPluginModules(services);
        }

        private static async Task StartPluginModules(IServiceProvider services, IEnumerable<IPlugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                await module.StartModuleAsync(services);
            }
        }

        private async Task RunPluginModules(IServiceProvider services)
        {
            foreach (IPluginModule module in _builder.AllModules)
            {
                await module.RunModuleAsync(services);
            }
        }
        
        
        //-----------------------------------------------------
        //--Runtime Services
        //-----------------------------------------------------
        
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
        
        // Creates a validation instance, based on the application configuration, used to validate an object.
        // The host application can specify an implementation using a validation library of choice.
        public IObjectValidator CreateValidator(object obj)
        {
            ThrowIfStopped();

            ValidationConfig validationConfig = _builder.GetContainerConfig<ValidationConfig>();
            return (IObjectValidator)Activator.CreateInstance(validationConfig.ValidatorType, obj);
        }
        
        //-----------------------------------------------------
        //--Stopping Composite Application
        //-----------------------------------------------------
        
        // Should be called when the application-host is stopped.  Each registered plugin-module
        // is stopped allowing it to reclaim resources.
        public async Task StopAsync()
        {
            if (!_isStarted)
            {
                return;
            }
            
            try
            {
                // Create a service scope in which each plugin can be started:
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapStop, "Stopping Composite-Application"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    await StopPluginModulesAsync(scope.ServiceProvider);
                }
            }
            catch (ContainerException ex)
            {
                LogException(ex);
                throw;
            }
            catch (AggregateException ex)
            {
                var flattenedEx = ex.Flatten();
                throw LogException(new ContainerException(
                    "Error stopping composite-application.  See Inner Exception.", flattenedEx));
                
            }
            catch (Exception ex)
            {
                throw LogException(new ContainerException(
                    "Error stopping composite-application.  See Inner Exception.", ex));
            }
        }
        
        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }
        
        private async Task StopPluginModulesAsync(IServiceProvider services)
        {
            _isStarted = false;
            
            var hostStopTask = StopPluginModules(services, new[] { _builder.HostPlugin });
            var appStopTask = StopPluginModules(services, _builder.AppPlugins);
            var coreStopTask = StopPluginModules(services, _builder.CorePlugins);

            await Task.WhenAll(hostStopTask, appStopTask, coreStopTask);
        }

        private static async Task StopPluginModules(IServiceProvider services, IEnumerable<IPlugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                await module.StopModuleAsync(services);
            }
        }
        
        private Exception LogException(Exception ex)
        {
            _logger.LogErrorDetails(BootstrapLogEvents.BootstrapException, ex, "Composite-Application Exception");
            return ex;
        }
        
        private void ThrowIfStopped()
        {
            if (! _isStarted)
            {
                throw new ContainerException(
                    "The composite-application has been stopped and can no longer be accessed.");
            }
        }
    }
}