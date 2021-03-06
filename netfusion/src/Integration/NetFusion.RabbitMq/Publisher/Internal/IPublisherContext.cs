using Microsoft.Extensions.Logging;
using NetFusion.Base.Serialization;
using NetFusion.RabbitMQ.Plugin;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Interface defining common services needed by called components
    /// implementing the publishing of messages.
    /// </summary>
    internal interface IPublisherContext
    {
        ILoggerFactory LoggerFactory { get; }
        
        // Modules:
        IBusModule BusModule { get; }
        IPublisherModule PublisherModule { get; }
        
        // Services:
        ISerializationManager Serialization { get; }
    }
}