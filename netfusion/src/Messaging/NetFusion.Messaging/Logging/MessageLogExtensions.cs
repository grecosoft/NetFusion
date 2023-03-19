using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Messaging.Logging;

/// <summary>
/// Provides extension methods related to the registering of message-logger services.
/// </summary>
public static class MessageLogExtensions
{
    /// <summary>
    /// Registers a message log sink called when a message is recorded by IMessageLogger.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <typeparam name="TSink">The type of the sink to be registered.</typeparam>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddMessageLogSink<TSink>(this IServiceCollection services)
        where TSink: class, IMessageLogSink
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
            
        services.AddSingleton<IMessageLogSink, TSink>();
        return services;
    }
        
    /// <summary>
    /// Registers a message log sink called when a message is recorded by IMessageLogger.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="messageLogSink">Reference to the sink to register.</param>
    /// <returns></returns>
    public static IServiceCollection AddMessageLogSink(this IServiceCollection services,
        IMessageLogSink messageLogSink)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (messageLogSink == null) throw new ArgumentNullException(nameof(messageLogSink));

        services.AddSingleton(messageLogSink);
        return services;
    }
}