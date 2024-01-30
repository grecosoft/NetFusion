using Azure.Messaging.ServiceBus;
using NetFusion.Common.Base;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Internal;
using NetFusion.Integration.ServiceBus.Plugin;

namespace NetFusion.Integration.ServiceBus.Namespaces;

public class NamespaceEntityContext : BusEntityContext
{
    private readonly ILogger _logger;
    
    /// <summary>
    /// Reference to the module containing namespace services.
    /// </summary>
    public INamespaceModule NamespaceModule { get; }
    
    public NamespaceEntityContext(IPlugin hostPlugin, IServiceProvider serviceProvider) : 
        base(hostPlugin, serviceProvider)
    {
        _logger = LoggerFactory.CreateLogger<NamespaceEntityContext>();
        NamespaceModule = serviceProvider.GetRequiredService<INamespaceModule>();
    }

    /// <summary>
    /// Determines if the Queue and Topic namespace entities should be automatically
    /// created when the microservice is bootstrapped.
    /// </summary>
    public bool IsAutoCreateEnabled => NamespaceModule.BusPluginConfiguration.IsAutoCreateEnabled;
    
    /// <summary>
    /// Deserializes a received Service Bus message.
    /// </summary>
    /// <param name="dispatcher">The dispatcher used to determine message's type to be deserialized into.</param>
    /// <param name="messageEventArgs">Details about the received message.</param>
    /// <returns></returns>
    public IMessage DeserializeMessage(MessageDispatcher dispatcher, ProcessMessageEventArgs messageEventArgs)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(messageEventArgs);

        BinaryData messageData = messageEventArgs.Message.Body;
        string contentType = messageEventArgs.Message.ContentType ?? ContentTypes.Json;
            
        var message = (IMessage?)Serialization.Deserialize(contentType, dispatcher.MessageType, messageData.ToArray());
        if (message == null)
        {
            throw new ServiceBusPluginException(
                $"Message Type {dispatcher.MessageType} could not be deserialized.");
        }
        return message;
    }
    
    /// <summary>
    /// Common method for logging message processing errors.
    /// </summary>
    /// <param name="eventArgs">Error information about the message being processed.</param>
    public void LogProcessError(ProcessErrorEventArgs eventArgs)
    {
        _logger.Log(LogLevel.Error, eventArgs.Exception, 
            "Processing error of source {ErrorSource} for the entity {EntityPath} within namespace {Namespace} received.", 
            eventArgs.ErrorSource,
            eventArgs.EntityPath, 
            eventArgs.FullyQualifiedNamespace);
    }
    
    public ServiceBusMessage CreateServiceBusMessage(string contentType, IMessage message)
    {
        byte[] messageData = Serialization.Serialize(message, contentType);
        var busMessage = new ServiceBusMessage(new BinaryData(messageData))
        {
            ContentType = contentType
        };
        
        SetBusMessageProps(busMessage, message);
        return busMessage;
    }
    
    private static void SetBusMessageProps(ServiceBusMessage busMessage, IMessage message)
    {
        SetIdentityPropsFromMessage(busMessage, message);
        SetTimeBasedPropsFromMessage(busMessage, message);
        SetDescriptivePropsFromMessage(busMessage, message);
    }

    private static void SetIdentityPropsFromMessage(ServiceBusMessage busMessage, IMessage message)
    {
        busMessage.MessageId = message.GetMessageId() ?? Guid.NewGuid().ToString();

        string? correlationId = message.GetCorrelationId();
        if (correlationId != null)
        {
            busMessage.CorrelationId = correlationId;
        }
    }

    private static void SetTimeBasedPropsFromMessage(ServiceBusMessage busMessage, IMessage message)
    {
        TimeSpan? timeToLive = message.GetTimeToLive();
        if (timeToLive != null)
        {
            busMessage.TimeToLive = timeToLive.Value;
        }
    }

    private static void SetDescriptivePropsFromMessage(ServiceBusMessage busMessage, IMessage message)
    {
        string? subject = message.GetSubject();
        if (subject != null)
        {
            busMessage.Subject = subject;
        }
    }
}