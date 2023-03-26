using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Logging;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Health;
using NetFusion.Core.Bootstrap.Logging;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Resulting Composite-Application built from a set of plugins.  A reference to a singleton instance
/// of this class can be referenced by injecting ICompositeApp into a dependent service component.
/// If the dependent component is not registered within the container, the Composite-Application can
/// be accessed using the CompositeApp.Instance property.
/// </summary>
public class CompositeApp : ICompositeApp
{
    // Singleton instance of the composite-application:
    private static ICompositeApp? _instance;
        
    public PluginSummary HostPlugin { get; }

    // Composite Application Statuses:
    public bool IsStarted { get; private set; }
    public bool IsReady { get; private set; }
        
    // Reference to the builder details that constructed the composite-application.
    private readonly ICompositeAppBuilder _builder;
        
    // The service-provider associated with the application created from the
    // service-collection populated by the bootstrapped plugin modules.
    private readonly IServiceProvider _serviceProvider;
        
    private readonly ILogger<CompositeApp> _logger;
    private readonly HealthCheckBuilder _healthCheckBuilder;

    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
        
    public CompositeApp(
        ICompositeAppBuilder builder,
        IServiceProvider serviceProvider,
        ILogger<CompositeApp> logger)
    {
        _instance = this;

        IsReady = false;
        IsStarted = false;
            
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        HostPlugin = new PluginSummary(builder.HostPlugin);

        _healthCheckBuilder = new HealthCheckBuilder(_builder.AllModules);
    }

    public static ICompositeApp Instance => _instance ?? 
                                            throw new BootstrapException("Composite Application has not been created.");


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
            throw new BootstrapException("Composite Application already started");
        }

        try
        {
            // Create a service scope in which each plugin can be started:
            using (_logger.LogInformationDuration("Starting Composite-Application"))
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {                    
                NfExtensions.Logger.Log<CompositeApp>(CoreServicesLogger.Log(scope.ServiceProvider));
                await StartCompositeApp(scope.ServiceProvider);
            }
        }
        catch (BootstrapException ex)
        {
            NfExtensions.Logger.LogError<CompositeApp>(ex, startExMsg);
            throw;
        }
        catch (AggregateException ex)
        {
            var flattenedEx = ex.Flatten();
                
            NfExtensions.Logger.LogError<CompositeApp>(flattenedEx, startExMsg);
            throw new BootstrapException(startExMsg, flattenedEx);
                
        }
        catch (Exception ex)
        {
            NfExtensions.Logger.LogError<CompositeApp>(ex, startExMsg);
            throw new BootstrapException(startExMsg, ex);
        }

        IsStarted = true;
        IsReady = true;
    }

    public void Start()
    {
        StartAsync().GetAwaiter().GetResult();
    }
        
    private async Task StartCompositeApp(IServiceProvider services)
    {
        // Now that the service-provider has been created, initialize each
        // module so it can use ILoggerFactory to obtain a logger.
        foreach (IPluginModule module in _builder.AllModules)
        {
            module.Context.InitLogging(services);
        }

        // Start the plug-in modules in dependent order starting with core modules 
        // and ending with the application host module.
        await StartPlugins(services);
            
        _logger.LogInformation("All Modules Started");

        // Last phase to allow any modules to execute any processing that
        // might be dependent on another module being started.
        await RunPlugins(services);
            
        _logger.LogInformation("All Modules Ran");
    }

    private async Task StartPlugins(IServiceProvider services)
    {
        await Task.WhenAll(_builder.CorePlugins.Select(p => p.StartAsync(_logger, services)));
        await Task.WhenAll(_builder.AppPlugins.Select(p => p.StartAsync(_logger, services)));
        await _builder.HostPlugin.StartAsync(_logger, services);
    }

    private async Task RunPlugins(IServiceProvider services)
    {
        await Task.WhenAll(_builder.CorePlugins.Select(p => p.RunAsync(_logger, services)));
        await Task.WhenAll(_builder.AppPlugins.Select(p => p.RunAsync(_logger, services)));
        await _builder.HostPlugin.RunAsync(_logger, services);
    }

    // --------------------------- [Runtime Services] -------------------------------
        
    public IDictionary<string, object> Log
    {
        get
        {
            ThrowIfStopped();
            return _builder.CompositeLog.GetLog();
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

    public string ToggleReadyStatus()
    {
        IsReady = !IsReady;
        return IsReady ? "READY" : "NOT-READY";
    }

    // Returns the current health of the Composite Application based on the
    // plugins from which it was built.
    public async Task<CompositeAppHealthCheck> GetHealthCheckAsync()
    {
        if (!IsStarted)
        {
            _logger.LogWarning("Composite Application reports Unhealthy until started.");
            return new CompositeAppHealthCheck(HealthCheckStatusType.Unhealthy);
        }
        
        var healthCheck = await _healthCheckBuilder.QueryHealthAsync();
        if (healthCheck.CompositeAppHealth == HealthCheckStatusType.Healthy)
        {
            return healthCheck;
        }
        
        var log = LogMessage.For(LogLevel.Debug,
                "Current Composite Application Health: {Status}",
                healthCheck.CompositeAppHealth)
            .WithProperties(LogProperty.ForName("HealthCheck", healthCheck));

        _logger.Log(log);
        return healthCheck;
    }

    // --------------------------- [Stopping Composite Application] -------------------------------
        
    // Should be called when the application-host is stopped.  Each registered
    // plugin-module is stopped allowing it to reclaim resources.
    public async Task StopAsync()
    {
        const string stopExMsg = "Error Stopping Composite Application";

        IsReady = false;
            
        if (! IsStarted)
        {
            return;
        }
            
        IsStarted = false;
            
        try
        {
            _logger.LogInformation("Composite Application Stopping.");
                
            // Create a service scope in which each plugin can be stopped:
            using (_logger.LogInformationDuration("Stopping Composite-Application"))
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                // Stops the plugins in the reverse order from which they where started.
                await StopCompositeApp(scope.ServiceProvider);
            }
        }
        catch (BootstrapException ex)
        {
            NfExtensions.Logger.LogError<CompositeApp>(ex, stopExMsg);
            throw;
        }
        catch (AggregateException ex)
        {
            var flattenedEx = ex.Flatten();
                
            NfExtensions.Logger.LogError<CompositeApp>(flattenedEx, stopExMsg);
            throw new BootstrapException(stopExMsg, flattenedEx);
        }
        catch (Exception ex)
        {
            NfExtensions.Logger.LogError<CompositeApp>(ex, stopExMsg);
            throw new BootstrapException(stopExMsg, ex);
        }
    }
        
    public void Stop()
    {
        StopAsync().GetAwaiter().GetResult();
    }
        
    private async Task StopCompositeApp(IServiceProvider services)
    {
        await _builder.HostPlugin.StopAsync(_logger, services);
        await Task.WhenAll(_builder.AppPlugins.Select(p => p.StopAsync(_logger, services)));
        await Task.WhenAll(_builder.CorePlugins.Select(p => p.StopAsync(_logger, services)));
    }

    private void ThrowIfStopped()
    {
        if (! IsStarted)
        {
            throw new BootstrapException("Stopped Composite Application can not be accessed");
        }
    }
}