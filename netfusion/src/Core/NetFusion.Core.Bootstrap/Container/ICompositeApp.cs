using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Health;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Represents an application composed from a set of plugins and available
/// for the lifetime of the executing host.
/// </summary>
public interface ICompositeApp
{
    /// <summary>
    /// Summary information for the host plugin that composed the composite application.
    /// </summary>
    PluginSummary HostPlugin { get; }

    /// <summary>
    /// Indicates that the application has been started.
    /// </summary>
    bool IsStarted { get; }
        
    /// <summary>
    /// Indicates that all modules have started and are ready.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Properties associated with the composite application used as a shared location that can
    /// be referenced by multiple components at different points in the application's life-cycle.
    /// </summary>
    IDictionary<string, object> Properties { get; }

    /// <summary>
    /// Indicates the overall health of the composite application based on its
    /// contained plugins and modules.
    /// </summary>
    Task<CompositeAppHealthCheck> GetHealthCheckAsync();
        
    /// <summary>
    /// Log containing details of how the application was composed from plugins.
    /// </summary>
    IDictionary<string, object> Log { get; }
        
    /// <summary>
    /// Starts each plugin module.
    /// </summary>
    /// <returns>Task that can be awaited after which all plugin modules will have been started.</returns>
    Task StartAsync();

    /// <summary>
    /// Starts each plugin module.
    /// </summary>
    void Start();

    /// <summary>
    /// Allows for service-location from a component that is not registered in the container.
    /// </summary>
    /// <returns>Service scope from which services can be resolved.  The scope must be disposed after use.</returns>
    IServiceScope CreateServiceScope();

    /// <summary>
    /// Stops each plugin module.
    /// </summary>
    /// <returns>Task that can be awaited after which all plugin modules will have been stopped.</returns>
    Task StopAsync();

    /// <summary>
    /// Stops each plugin module.
    /// </summary>
    void Stop();

    /// <summary>
    /// Toggles the current ready status.  This can be used when running within a component coordinator such
    /// as Kubernetes that monitors that current status of running microservices. 
    /// </summary>
    /// <returns>Return string value indicating status after being toggled.</returns>
    string ToggleReadyStatus();
}