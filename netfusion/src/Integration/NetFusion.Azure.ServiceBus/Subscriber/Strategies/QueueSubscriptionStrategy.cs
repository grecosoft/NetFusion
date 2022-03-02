using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Subscriber.Strategies
{
    /// <summary>
    /// Strategy subscribing and processing Command messages received on a queue.
    /// </summary>
    public class QueueSubscriptionStrategy : ISubscriptionStrategy,
        IRequiresContext
    {
        private readonly QueueSubscription _subscription;
        
        public QueueSubscriptionStrategy(QueueSubscription subscription)
        {
            _subscription = subscription;
        }
        
        public NamespaceContext Context { get; set; }

        public Task Subscribe(NamespaceConnection connection)
        {
            ServiceBusProcessor processor = connection.BusClient.CreateProcessor(
                _subscription.EntityName, 
                _subscription.Options);
          
            _subscription.ProcessedBy(processor);

            processor.ProcessMessageAsync += OnMessageReceived;
            processor.ProcessErrorAsync += OnProcessingError;
            
            return processor.StartProcessingAsync();
        }

        private async Task OnMessageReceived(ProcessMessageEventArgs args)
        {
            IMessage message = DeserializeReceivedMessage(args);
            
            Context.LogSubscription(_subscription);
            
            // Dispatch the received message to the associated in-process handler:
            object response = await Context.DispatchModule.InvokeDispatcherInNewLifetimeScopeAsync(
                _subscription.DispatchInfo, 
                message);

            // If the massage handler returned a response, and the originating sender
            // expects a reply, place the response message on the reply-to queue:
            if (response != null && NamespaceContext.TryParseReplyTo(args, 
                out string namespaceName, 
                out string queueName))
            {
                Context.Logger.LogDebug("Response {ResponseType} being sent to reply queue {ReplyQueue}", 
                    response.GetType(), 
                    $"{namespaceName}:{queueName}");
                
                ServiceBusMessage responseMsg = SerializeResponse(args, response);
                NamespaceConnection connection = Context.NamespaceModule.GetConnection(namespaceName);

                await using var publisher = connection.BusClient.CreateSender(queueName);
                await publisher.SendMessageAsync(responseMsg);
            }
        }

        private IMessage DeserializeReceivedMessage(ProcessMessageEventArgs args)
        {
            IMessage message = Context.DeserializeMessage(_subscription.DispatchInfo, args);
            
            // If this is a replay response to a prior command, set the CorrelationId and MessageId from the bus message.
            // This allows the message handler to correlate the replay to the original request.
            message.SetMessageId(args.Message.MessageId);
            message.SetCorrelationId(args.Message.CorrelationId);
            message.SetContentType(args.Message.ContentType);

            if (! string.IsNullOrWhiteSpace(args.Message.ReplyTo))
            {
                message.SetReplyTo(args.Message.ReplyTo);
            }
            
            return message;
        }
        
        private ServiceBusMessage SerializeResponse(ProcessMessageEventArgs args, object response)
        {
            // The response will be serialized in the same format as the received message request
            // and the correlation Id of the request message will be set on the response message.
            byte[] messageData = Context.Serialization.Serialize(response, args.Message.ContentType);
            
            return new ServiceBusMessage(new BinaryData(messageData))
            {
                ContentType = args.Message.ContentType, 
                MessageId = args.Message.MessageId,
                CorrelationId = args.Message.CorrelationId
            };
        } 
        
        private Task OnProcessingError(ProcessErrorEventArgs args)
        {
            Context.LogProcessError(args);
            return Task.CompletedTask;
        }
    }
}