using Azure.Messaging.ServiceBus;
using NetFusion.Integration.Bus;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Plugin;

namespace NetFusion.Integration.ServiceBus.Queues;

/// <summary>
/// Service allowing a response to a previously received command to be send back
/// to the originating microservice.  This can be used if the microservice processing
/// the command saves it for later processing and needs to send a response to the
/// originating microservice.
/// </summary>
internal class QueueResponseService: IQueueResponseService
{
    private readonly ILogger<QueueResponseService> _logger;
    private readonly INamespaceModule _namespaceModule;
    private readonly ISerializationManager _serialization;
    
    public QueueResponseService(ILogger<QueueResponseService> logger,
        INamespaceModule namespaceModule, 
        ISerializationManager serialization)
    {
        _logger = logger;
        _namespaceModule = namespaceModule;
        _serialization = serialization;
    }
    
    public async Task RespondToSenderAsync(ICommand command, ICommand response)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));
        if (response == null) throw new ArgumentNullException(nameof(response));
        
        // Validate that the original received command for which the response
        // is being sent is marked with a correctly formatted ReplyTo value.

        if (! command.TryParseReplyTo(out var namespaceName, out var queueName))
        {
            _logger.LogError(
                "The request of type: {CommandType} does not have a valid ReplyTo value specifying " +
                "the namespace/queue to send response of type: {ResponseType}", 
                command.GetType(), 
                response.GetType());
            
            return;
        }

        if (!ValidateOriginalCommand(command)) return;

        // Build and send the reply message on the ReplyTo queue:
        NamespaceConnection namespaceConn = _namespaceModule.GetConnection(namespaceName);
        ServiceBusMessage busMessage = BuildResponseMessage(command, response);

        try
        {
            await using var sender = namespaceConn.BusClient.CreateSender(queueName);
            await sender.SendMessageAsync(busMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to command with {MessageId}.", command.GetMessageId());
        }
    }

    // Validate the command is marked with the Content-Type and values used to identify the message
    // so the original sender can process and correlate the message back the original sent command.
    private bool ValidateOriginalCommand(IMessage command)
    {
        if (
            string.IsNullOrWhiteSpace(command.GetContentType()) ||
            string.IsNullOrWhiteSpace(command.GetMessageId()) ||
            string.IsNullOrWhiteSpace(command.GetCorrelationId()))
        {
            _logger.LogError(
                "The {CommandType} to send a response must have the ContentType, MessageId And CorrelationId.",
                command.GetType());
            return false;
        }

        return true;
    }
    
    private ServiceBusMessage BuildResponseMessage(ICommand command, ICommand response)
    {
        byte[] messageData = _serialization.Serialize(response, command.GetContentType()!);
        var busMessage = new ServiceBusMessage(new BinaryData(messageData));
            
        SetMessageProperties(command, busMessage);
        return busMessage;
    }

    private static void SetMessageProperties(IMessage message, ServiceBusMessage busMessage)
    {
        busMessage.ContentType = message.GetContentType();
        busMessage.MessageId = message.GetMessageId();
        busMessage.CorrelationId = message.GetCorrelationId();
    }
}
