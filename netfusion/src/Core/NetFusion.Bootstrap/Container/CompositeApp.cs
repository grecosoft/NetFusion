using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Base.Serialization;
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
        public PluginSummary HostPlugin { get; }

        // Singleton instance of the composite-application:
        public static ICompositeApp Instance { get; private set; }
        public bool IsStarted { get; private set; }
        
        // Reference to the builder details that constructed this composite-application.
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
            HostPlugin = new PluginSummary(builder.HostPlugin);

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
            if (IsStarted)
            {
                throw LogException(new ContainerException(
                    "The Composite-Application has already been started."));
            }

            // Write the logs that were recorded before the container-provider
            // was built now that ILogger is available.
            _builder.BootstrapLogger.WriteToLogger(_logger);
            
            // If there were any bootstrap errors,  raise an exception to 
            // abort starting the composite-application.
            if (_builder.BootstrapLogger.HasErrors)
            {
                throw new ContainerException(
                    "Errors were recorded when bootstrapping Composite-Application.  See log for details.");
            }
            
            try
            {
                _logger.LogInformation("Composite-Application Starting.");
               
                // Create a service scope in which each plugin can be started:
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapStart, "Starting Modules"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    LogCoreServices(scope.ServiceProvider);
                    
                    await StartModules(scope.ServiceProvider);
                }
               
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTraceDetails(BootstrapLogEvents.BootstrapCompositeLog, "Composite-Log", Log);
                }
                
                _logger.LogInformation("CompositeApplication Started");
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
                    "Error starting Composite-Application.  See Inner Exception.", flattenedEx));
                
            }
            catch (Exception ex)
            {
                throw LogException(new ContainerException(
                    "Error starting Composite-Application. See Inner Exception.", ex));
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

            // Allow each module context to be initialized with the singleton logging
            // services only available after the service-provider has been created.
            foreach (IPluginModule module in _builder.AllModules)
            {
                module.Context.InitLogging(services);
            }
     
            var coreStartTask = StartPluginModules(services, _builder.CorePlugins);
            var appStartTask = StartPluginModules(services, _builder.AppPlugins);
            var hostStartTask = StartPluginModules(services, new[] { _builder.HostPlugin });

            await Task.WhenAll(coreStartTask, appStartTask, hostStartTask);

            // Last phase to allow any modules to execute any processing that
            // might be dependent on another module being started.
            await RunPluginModules(services);
        }

        private async Task StartPluginModules(IServiceProvider services, IEnumerable<IPlugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                _logger.LogDebug("Starting Module: {moduleType} for Plugin: {pluginName}", 
                    module.GetType().Name, 
                    module.Context.Plugin.Name);
                
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

        private void LogCoreServices(IServiceProvider provider)
        {
            var validationConfig = _builder.GetContainerConfig<ValidationConfig>();
            var serializerMgr = provider.GetService<ISerializationManager>();
            var scriptingSrv = provider.GetService<IEntityScriptingService>();

            if (serializerMgr == null)
            {
                // This should never happen since the CompositeAppBuilder adds a default implementation:
                _logger.LogWarning(
                    "Serializer Manager Service not Registered.");
            }
            else
            {
                _logger.LogDebug("Registered Serializer Manager: {serializer}", 
                    serializerMgr.GetType().AssemblyQualifiedName);
            }

            if (scriptingSrv == null)
            {
                // This should never happen since the CompositeAppBuilder adds a default implementation:
                _logger.LogWarning(
                    "Scripting Service not Registered.");
            }
            else
            {
                _logger.LogDebug("Registered Scripting Service: {serializer}", 
                    scriptingSrv.GetType().AssemblyQualifiedName);
            }
            
            _logger.LogDebug("Registered Object Validator: {validator}",
                validationConfig.ValidatorType.AssemblyQualifiedName);
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
        
        // Creates a validation instance, based on the application configuration used to validate an object.
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
        
        // Should be called when the application-host is stopped.  Each registered
        // plugin-module is stopped allowing it to reclaim resources.
        public async Task StopAsync()
        {
            if (! IsStarted)
            {
                return;
            }
            
            try
            {
                _logger.LogInformation("Composite Application Stopping.");
                
                // Create a service scope in which each plugin can be stopped:
                using (_logger.LogTraceDuration(BootstrapLogEvents.BootstrapStop, "Stopping Composite-Application"))
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    await StopPluginModulesAsync(scope.ServiceProvider);
                }
                
                _logger.LogInformation("Composite Application Stopped.");
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
            IsStarted = false;
            
            var hostStopTask = StopPluginModules(services, new[] { _builder.HostPlugin });
            var appStopTask = StopPluginModules(services, _builder.AppPlugins);
            var coreStopTask = StopPluginModules(services, _builder.CorePlugins);

            await Task.WhenAll(hostStopTask, appStopTask, coreStopTask);
        }

        private async Task StopPluginModules(IServiceProvider services, IEnumerable<IPlugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                _logger.LogDebug("Stopping Module: {moduleType} for Plugin: {pluginName}", 
                    module.GetType().Name, 
                    module.Context.Plugin.Name);
                
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
            if (! IsStarted)
            {
                throw new ContainerException(
                    "The composite-application has been stopped and can no longer be accessed.");
            }
        }
    }
}