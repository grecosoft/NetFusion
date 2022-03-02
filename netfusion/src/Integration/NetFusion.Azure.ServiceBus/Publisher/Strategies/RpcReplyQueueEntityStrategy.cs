using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Namespaces;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
using System;
using System.Threading.Tasks;

namespace NetFusion.Azure.ServiceBus.Publisher.Strategies
{
    /// <summary>
    /// Custom publish strategy containing logic coordinating the publishing
    /// of commands and processing the corresponding reply response.
    /// </summary>
    public class RpcReplyQueueEntityStrategy : IEntityStrategy,
        IRequiresContext,
        ICleanupStrategy
    {
        private readonly RpcReplyQueryMeta _rpcReplyQueueMeta;


        // Process RPC reply messages and updates state of RpcPendingRequest. 
        private ServiceBusProcessor _replyProcessor;
        
        public RpcReplyQueueEntityStrategy(RpcReplyQueryMeta rpcReplyQueueMeta)
        {
            _rpcReplyQueueMeta = rpcReplyQueueMeta;
        }
        
        public NamespaceContext Context { get; set; }
                

        // ------------------ RPC Reply Responses -------------------
        
        // Creates a Reply Queue used to receive responses to RPC based commands.
        public async Task CreateEntityAsync(NamespaceConnection connection)
        {
            var options = new CreateQueueOptions(_rpcReplyQueueMeta.UniqueReplyQueueName)
            {
                // In case the publishing microservice was not stopped cleanly, configure
                // the replay queue to be automatically deleted when all subscriptions no
                // longer exist.
                AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
            };

            await connection.AdminClient.CreateQueueAsync(options);
            await SubscribeToReplyQueue(connection);
        }
        
        public async Task CleanupEntityAsync(NamespaceConnection connection)
        {
            if (_replyProcessor != null)
            {
                await _replyProcessor.CloseAsync();
            }
            
            await connection.AdminClient.DeleteQueueAsync(_rpcReplyQueueMeta.UniqueReplyQueueName);
        }
        
        private Task SubscribeToReplyQueue(NamespaceConnection connection)
        {
            _replyProcessor = connection.BusClient.CreateProcessor(_rpcReplyQueueMeta.UniqueReplyQueueName,
                new ServiceBusProcessorOptions
                {
                    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                    MaxConcurrentCalls = 40
                });
            
            _replyProcessor.ProcessMessageAsync += OnMessageReceived;
            _replyProcessor.ProcessErrorAsync += OnProcessingError;
            
            return _replyProcessor.StartProcessingAsync();
        }

        private Task OnMessageReceived(ProcessMessageEventArgs args)
        {
            string messageId = args.Message.MessageId;
                    
            if (string.IsNullOrWhiteSpace(messageId))
            {
                Context.Logger.LogError("The received reply message does not have a Message Id.");
                return Task.CompletedTask;
            }

            Context.Logger.LogDebug(
                "Received Response for Command with Message Id {MessageId}.", messageId);

            if (! _rpcReplyQueueMeta.PendingRpcRequests.TryRemove(messageId, out RpcPendingRequest pendingRequest))
            {
                Context.Logger.LogError("The received Message Id: {MessageId} does not have pending request.", messageId);
                return Task.CompletedTask;       
            }
            
            Context.Logger.LogDebug("Message received on RPC reply queue {QueueName} with Message Id {MessageId}",
                _rpcReplyQueueMeta.UniqueReplyQueueName, messageId);

            SetReplyResult(pendingRequest, args);
            return Task.CompletedTask;
        }

        private static void SetReplyResult(RpcPendingRequest pendingRequest, ProcessMessageEventArgs args)
        {
            var replyEx = CheckReplyException(args);
            if (replyEx != null)
            {
                pendingRequest.SetException(replyEx);
                return;
            }
            
            pendingRequest.SetResult(args.Message.Body.ToArray());
        }

        private static RpcReplyException CheckReplyException(ProcessMessageEventArgs args)
        {
            if (args.Message.ApplicationProperties.TryGetValue("RpcError", out object value))
            {
                return string.IsNullOrEmpty(value.ToString())
                    ? new RpcReplyException("RPC Queue subscriber indicated error without details.")
                    : new RpcReplyException(value.ToString());
            }

            return null;
        }

        private Task OnProcessingError(ProcessErrorEventArgs args)
        {
            Context.LogProcessError(args);
            return Task.CompletedTask;
        }
    }
}