using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Rpc.Metadata;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.ServiceBus.Rpc.Strategies;

/// <summary>
/// Allows a microservice to create a queue on which it will receive RPC messages from another
/// microservice.  To make the implementation more efficient, multiple RPC commands types are
/// set on the same queue identified by a message-namespace message property.  This property
/// is used to route the message to the correct consumer message handler.  The result returned
/// from the message handler is then returned on the result queue specified on the message.
/// </summary>
internal class RpcConsumerStrategy : BusEntityStrategyBase<NamespaceEntityContext>, 
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly RpcEntity _rpcEntity;
    private ServiceBusProcessor? _queueProcessor;

    public RpcConsumerStrategy(RpcEntity rpcEntity)
    {
        _rpcEntity = rpcEntity;
    }
    
    private ILogger<RpcConsumerStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<RpcConsumerStrategy>();
    
    public Task CreateEntity()
    {
        if (!Context.IsAutoCreateEnabled) return Task.CompletedTask;
        
        var connection = Context.NamespaceModule.GetConnection(_rpcEntity.BusName);
        CreateQueueOptions queueOptions = BuildQueueOptions(_rpcEntity.EntityName, _rpcEntity.RpcQueueMeta);
        return connection.CreateOrUpdateQueue(_rpcEntity.EntityName, queueOptions);
    }

    private static CreateQueueOptions BuildQueueOptions(string queueName, IRpcQueueMeta queueMeta)
    {
        var defaults = new CreateQueueOptions(queueName);
        
        return new CreateQueueOptions(queueName)
        {
            LockDuration = queueMeta.LockDuration ?? defaults.LockDuration,
            MaxDeliveryCount = queueMeta.MaxDeliveryCount ?? defaults.MaxDeliveryCount,
            MaxSizeInMegabytes = queueMeta.MaxSizeInMegabytes ?? defaults.MaxSizeInMegabytes,
            DefaultMessageTimeToLive = queueMeta.DefaultMessageTimeToLive ?? defaults.DefaultMessageTimeToLive
        };
    }
    
    public Task SubscribeEntity()
    {
        var connection = Context.NamespaceModule.GetConnection(_rpcEntity.BusName);
        var processingOptions = _rpcEntity.RpcQueueMeta.ProcessingOptions;
        
        connection.ApplyExternalRpcQueueProcessingSettings(_rpcEntity.EntityName, processingOptions);
        
        _queueProcessor = connection.BusClient.CreateProcessor(_rpcEntity.EntityName, 
            new ServiceBusProcessorOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
                MaxConcurrentCalls = processingOptions.MaxConcurrentCalls,
                PrefetchCount = processingOptions.PrefetchCount,
                Identifier = processingOptions.Identifier
            });

        _queueProcessor.ProcessMessageAsync += OnMessageReceived;
        _queueProcessor.ProcessErrorAsync += OnProcessingError;
        
        return _queueProcessor.StartProcessingAsync();
    }

    private async Task OnMessageReceived(ProcessMessageEventArgs args)
    {
        var messageDispatcher = GetMessageDispatcher(args);
        if (messageDispatcher == null)
        {
            await ReplyWithError(args, "The message was received but could not be dispatched using provided namespace.");
            return;
        }
        
        // Deserialize received message and dispatch to command handler:
        IMessage message = Context.DeserializeMessage(messageDispatcher, args);
        
        var msgLog = new MessageLog(LogContextType.ReceivedMessage, message);
        msgLog.SentHint("service-bus-rpc-queue");
        
        LogReceivedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            object? response = await Context.MessageDispatcher.InvokeDispatcherInNewLifetimeScopeAsync(
                messageDispatcher,
                message);

            if (response == null)
            {
                Logger.LogWarning("{MessageDispatcher} returned null response.", messageDispatcher.ToString());

                await ReplyWithError(args,
                    "A null response message was returned in response to RPC request.");
                return;
            }

            await ReplyWithResponse(args, response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error dispatching RPC message {MessageType} received on queue {QueueName} defined on {Bus}.",
                message.GetType().Name,
                _rpcEntity.EntityName,
                _rpcEntity.BusName);

            msgLog.AddLogError(nameof(RpcConsumerStrategy), ex);
            await ReplyWithError(args, ex.ToString());
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private MessageDispatcher? GetMessageDispatcher(ProcessMessageEventArgs args)
    {
        if (! args.Message.ApplicationProperties.TryGetValue("MessageNamespace", out object? messageNs))
        {
            Logger.LogError(
                "Received RPC Command delivered to {Queue} on {Namespace} does not specify a Message Namespace " +
                "identifying the Command.", _rpcEntity.EntityName, _rpcEntity.BusName);
            return null;
        }

        if (! _rpcEntity.TryGetMessageDispatcher((string)messageNs, out MessageDispatcher? dispatcher))
        {
            Logger.LogError(
                "A message dispatcher for {MessageNamespace} delivered to {Queue} on {Namespace} is not configured",
                messageNs, _rpcEntity.EntityName, _rpcEntity.BusName);
        }

        return dispatcher;
    }
    
    private async Task ReplyWithError(ProcessMessageEventArgs args, string errorMessage)
    {
        if (! MessageProperties.TryParseReplyTo(args, out string? namespaceName, out string? queueName))
        {
            return;
        }
            
        NamespaceConnection connection = Context.NamespaceModule.GetConnection(namespaceName);

        // Add application properties to indicate and describe the error.
        // The publisher can then use the error method to throw an exception.
        var errorMsg = new ServiceBusMessage
        {
            CorrelationId = args.Message.CorrelationId,
            MessageId = args.Message.MessageId,
            ApplicationProperties =
            {
                ["RpcError"] = errorMessage
            }
        };

        await using var publisher = connection.BusClient.CreateSender(queueName);
        await publisher.SendMessageAsync(errorMsg);
    }
    
    private async Task ReplyWithResponse(ProcessMessageEventArgs args, object response)
    {
        if (!MessageProperties.TryParseReplyTo(args, out string? busName, out string? queueName))
        {
            throw new InvalidOperationException(
                $"The ReplyTo message property of: {args.Message.ReplyTo} does not specify " +
                "the name of the message bus and queue joined by a : character.");
        }
        
        ServiceBusMessage responseMsg = SerializeResponse(args, response);
        NamespaceConnection connection = Context.NamespaceModule.GetConnection(busName);

        var msgLog = new MessageLog(LogContextType.ReceivedMessage, response);
        msgLog.SentHint("service-bus-rpc-response");
        
        LogResponseMessage(response, args.Message.ReplyTo);

        try
        {
            await using var publisher = connection.BusClient.CreateSender(queueName);
            await publisher.SendMessageAsync(responseMsg);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError(nameof(RpcConsumerStrategy), ex);
            throw;
        }
        finally
        {
           await Context.MessageLogger.LogAsync(msgLog);
        }
    }
        
    private ServiceBusMessage SerializeResponse(ProcessMessageEventArgs args, object response)
    {
        // The response will be serialized in the same format as the received message request
        // and the MessageId of the request message will be set on the response message.
        byte[] messageData = Context.Serialization.Serialize(response, args.Message.ContentType);
            
        return new ServiceBusMessage(new BinaryData(messageData))
        {
            MessageId = args.Message.MessageId,
            ContentType = args.Message.ContentType,
            CorrelationId = args.Message.CorrelationId,
        };
    }

    private Task OnProcessingError(ProcessErrorEventArgs args)
    {
        Context.LogProcessError(args);
        return Task.CompletedTask;
    }

    public async Task OnDispose()
    {
        if (_queueProcessor != null)
        {
           await _queueProcessor.DisposeAsync();
        }
    }
    
    private void LogReceivedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Received from {Queue} on {Bus}",
            message.GetType(),
            _rpcEntity.EntityName,
            _rpcEntity.BusName).WithProperties(
            LogProperty.ForName("QueueInfo", _rpcEntity.GetLogProperties())
        );
            
        Logger.Log(log);
    }

    private void AddEntityDetailsToLog(MessageLog msgLog)
    {
        foreach ((string key, string? value) in _rpcEntity.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
    }
    
    private void LogResponseMessage(object response, string replyQueue)
    {
        Logger.LogDebug("Response {ResponseType} being sent to reply queue {ReplyQueue}", 
            response.GetType(), 
            replyQueue);
    }
}


