using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Plugin;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Subscriber.Internal
{
    public class QueueResponseService : IQueueResponseService
    {
        private readonly ILogger _logger;
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
        
        public async Task RespondToSenderAsync(ICommand command, object response)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (response == null) throw new ArgumentNullException(nameof(response));
            
            // Validate that the original received command for which the response
            // is being sent is marked with a correctly formatted ReplyTo value.

            if (! NamespaceContext.TryParseReplyTo(command, out var namespaceName, out var queueName))
            {
                _logger.LogError(
                    "The request of type: {CommandType} does not have a valid ReplyTo value specifying " +
                    "the namespace/queue to send response of type: {ResponseType}", 
                    command.GetType(), 
                    response.GetType());
                
                return;
            }
            
            // Validate the command is marked with the Content-Type and values identifying the message to 
            // the original sender can process and correlate the message back the the original send command.

            if (! ValidateOriginalCommand(command))
            {
                return;
            }
            
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
                _logger.LogError(ex, "Error replying to command with {CorrelationId}.", command.GetCorrelationId());
            }
        }

        private bool ValidateOriginalCommand(ICommand command)
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
        
        private ServiceBusMessage BuildResponseMessage(ICommand command, object response)
        {
            var messageData = SerializeMessage(command, response);
            var busMessage = new ServiceBusMessage(messageData);
                
            SetMessageProperties(command, busMessage);
            return busMessage;
        }
        
        private BinaryData SerializeMessage(ICommand command, object response)
        {
            byte[] messageData = _serialization.Serialize(response, command.GetContentType());
            return new BinaryData(messageData);
        }

        private static void SetMessageProperties(IMessage message, ServiceBusMessage busMessage)
        {
            busMessage.ContentType = message.GetContentType();
            busMessage.MessageId = message.GetMessageId();
            busMessage.CorrelationId = message.GetCorrelationId();
        }
    }
}