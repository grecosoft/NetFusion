using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Serialization;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.Bus.Entities;

/// <summary>
/// Defines a context providing services used by the strategies
/// for a given service-bus implementation.
/// </summary>
public abstract class BusEntityContext
{
    /// <summary>
    /// Reference to the plugin of the hosting microservice.
    /// </summary>
    public IPlugin HostPlugin { get; }
    
    /// <summary>
    /// Reference to logger factory used to create specific scoped loggers.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }
    
    /// <summary>
    /// Service used to serialize messages.
    /// </summary>
    public ISerializationManager Serialization { get; }
    
    /// <summary>
    /// Service used to invoke message consumers for received messages.
    /// </summary>
    public IMessageDispatcherService MessageDispatcher { get; }
    
    /// <summary>
    /// Service used to write message specific logs.
    /// </summary>
    public IMessageLogger MessageLogger { get; }

    protected BusEntityContext(IPlugin hostPlugin, IServiceProvider serviceProvider)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        
        HostPlugin = hostPlugin ?? throw new ArgumentNullException(nameof(hostPlugin));
        LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        Serialization = serviceProvider.GetRequiredService<ISerializationManager>();
        MessageDispatcher = serviceProvider.GetRequiredService<IMessageDispatcherService>();
        MessageLogger = serviceProvider.GetRequiredService<IMessageLogger>();
    }
}