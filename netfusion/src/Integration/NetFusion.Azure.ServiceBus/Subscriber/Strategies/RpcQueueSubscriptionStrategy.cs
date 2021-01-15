using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Subscriber.Strategies
{
    public class RpcQueueSubscriptionStrategy : ISubscriptionStrategy,
        IRequiresContext
    {
        private readonly RpcQueueSubscription _subscription;

        public RpcQueueSubscriptionStrategy(RpcQueueSubscription subscription)
        {
            _subscription = subscription;
        }
        
        public NamespaceContext Context { get; set; }

        // Subscribes to a Queue on which incoming RPC requests published. 
        public Task Subscribe(NamespaceConnection connection)
        {
            ServiceBusProcessor processor = connection.BusClient.CreateProcessor(_subscription.EntityName, _subscription.Options);
            
            _subscription.ProcessedBy(processor);

            processor.ProcessMessageAsync += OnMessageReceived;
            processor.ProcessErrorAsync += OnProcessingError;
            
            return processor.StartProcessingAsync();
        }

        private async Task OnMessageReceived(ProcessMessageEventArgs args)
        {
            MessageDispatchInfo dispatchInfo = GetCommandDispatchInfo(args);
            if (dispatchInfo == null)
            {
                await ReplyWithError(args, 
                    "The message was received but could not be dispatched.  Check destination server log.");
            }
            
            try
            {
                IMessage message = Context.DeserializeMessage(dispatchInfo, args);
                
                // Deserialize received message and dispatch to command handler:
                object response = await Context.DispatchModule.InvokeDispatcherInNewLifetimeScopeAsync(
                    dispatchInfo,
                    message);

                await ReplyWithResponse(args, response);
            }
            catch (Exception ex)
            {
                await ReplyWithError(args, ex.ToString());
                throw;
            }
        }
        
        private MessageDispatchInfo GetCommandDispatchInfo(ProcessMessageEventArgs args)
        {
            if (!args.Message.ApplicationProperties.TryGetValue("MessageNamespace", out object messageNs))
            {
                Context.Logger.LogError(
                    "Received RPC Command delivered to {Queue} on {Namespace} does not specify a Message Namespace " +
                    "identifying the Command.", _subscription.EntityName, _subscription.NamespaceName);
                return null;
            }
            
            MessageDispatchInfo dispatchInfo = _subscription.GetMessageNamespaceDispatch(messageNs.ToString());
            if (dispatchInfo == null)
            {
                Context.Logger.LogError(
                    "A Command handler for {MessageNamespace} delivered to {Queue} on {Namespace} is not configured",
                    messageNs, _subscription.EntityName, _subscription.NamespaceName);
            }

            return dispatchInfo;
        }

        private async Task ReplyWithResponse(ProcessMessageEventArgs args, object response)
        {
            if (response != null && NamespaceContext.TryParseReplyTo(args, 
                    out string namespaceName, 
                    out string queueName))
            {
                ServiceBusMessage responseMsg = SerializeResponse(args, response);
                NamespaceConnection connection = Context.NamespaceModule.GetConnection(namespaceName);

                await using var publisher = connection.BusClient.CreateSender(queueName);
                await publisher.SendMessageAsync(responseMsg);
            }
        }
        
        private ServiceBusMessage SerializeResponse(ProcessMessageEventArgs args, object response)
        {
            byte[] messageData = Context.Serialization.Serialize(response, args.Message.ContentType);
            
            return new ServiceBusMessage(new BinaryData(messageData))
            {
                // The response will be serialized in the same format as the received message request
                // and the correlation Id of the request message will be set on the response message.
                ContentType = args.Message.ContentType,
                CorrelationId = args.Message.CorrelationId
            };
        }

        private async Task ReplyWithError(ProcessMessageEventArgs args, string errorMessage)
        {
            if (! NamespaceContext.TryParseReplyTo(args, out string namespaceName, out string queueName))
            {
                return;
            }
            
            NamespaceConnection connection = Context.NamespaceModule.GetConnection(namespaceName);

            // Add application properties to indicate and describe the error.
            // The publisher can then use the error method to throw an exception.
            var errorMsg = new ServiceBusMessage
            {
                CorrelationId = args.Message.CorrelationId
            };
            
            errorMsg.ApplicationProperties["RpcError"] = errorMessage;
            
            await using var publisher = connection.BusClient.CreateSender(queueName);
            await publisher.SendMessageAsync(errorMsg);
        }
        
        private Task OnProcessingError(ProcessErrorEventArgs args)
        {
            Context.LogProcessError(args);
            return Task.CompletedTask;
        }
    }
}