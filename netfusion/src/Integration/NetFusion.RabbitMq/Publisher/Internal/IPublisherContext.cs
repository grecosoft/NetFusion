using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.RabbitMQ.Serialization;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Interface defining common services needed by called components
    /// implementing the publishing of messages.
    /// </summary>
    internal interface IPublisherContext
    {
        ILogger Logger { get; }
        IPublisherModule PublisherModule { get; }
        ISerializationManager Serialization { get; }
        IEntityScriptingService Scripting { get; }
    }
}