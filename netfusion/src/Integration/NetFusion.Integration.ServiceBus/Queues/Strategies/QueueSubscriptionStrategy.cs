using System.ComponentModel;
using Azure.Messaging.ServiceBus;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.ServiceBus.Queues.Strategies;

/// <summary>
/// Strategy that subscribes to a queue and routes received commands
/// to its associated message consumer and optionally sends a response
/// back to the originating microservice.
/// </summary>
internal class QueueSubscriptionStrategy : BusEntityStrategyBase<NamespaceEntityContext>,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly QueueEntity _queueEntity;
    private readonly MessageDispatcher _dispatcher;

    private ServiceBusProcessor? _queueProcessor;

    public QueueSubscriptionStrategy(QueueEntity queueEntity, MessageDispatcher dispatcher)
    {
        _queueEntity = queueEntity;
        _dispatcher = dispatcher;
    }
    
    private ILogger<QueueSubscriptionStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<QueueSubscriptionStrategy>();
    
    [Description("Subscribing to Queue for Command Processing.")]
    public Task SubscribeEntity()
    {
        var connection = Context.NamespaceModule.GetConnection(_queueEntity.BusName);
        
        connection.ApplyExternalQueueProcessingSettings(_queueEntity.EntityName,
            _queueEntity.QueueMeta.ProcessingOptions);

        _queueProcessor = connection.BusClient.CreateProcessor(
            _queueEntity.EntityName, 
            _queueEntity.QueueMeta.ProcessingOptions);

        _queueProcessor.ProcessMessageAsync += OnMessageReceived;
        _queueProcessor.ProcessErrorAsync += OnProcessingError;

        return _queueProcessor.StartProcessingAsync();
    }
    
    private async Task OnMessageReceived(ProcessMessageEventArgs args)
    {
        IMessage message = DeserializeReceivedMessage(args);
        
        var msgLog = new MessageLog(LogContextType.ReceivedMessage, message);
        msgLog.SentHint("service-bus-queue");
        
        LogReceivedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            // Dispatch the received message to the associated in-process consumer:
            IMessage? response = (IMessage?)await Context.MessageDispatcher.InvokeDispatcherInNewLifetimeScopeAsync(
                _dispatcher, 
                message,
                args.CancellationToken);

            if (response == null)
            {
                return;
            }

            await RespondToReplyQueue(args, response);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error dispatching message {MessageType} received on queue {QueueName} defined on {Bus}.",
                _dispatcher.MessageType.Name,
                _queueEntity.EntityName,
                _queueEntity.BusName);

            msgLog.AddLogError(nameof(QueueCreationStrategy), ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }

    private async Task RespondToReplyQueue(ProcessMessageEventArgs args, IMessage response)
    {
        if (!MessageProperties.TryParseReplyTo(args, out string? busName, out string? queueName))
        {
            return;
        }
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, response);
        msgLog.SentHint("rabbitmq-send-reply-queue");
        
        LogResponseMessage(response, args.Message.ReplyTo);
        AddResponseDetailsToLog(msgLog, args.Message.ReplyTo);

        try
        {
            ServiceBusMessage responseMsg = CreateResponseMessage(args, response);
            NamespaceConnection connection = Context.NamespaceModule.GetConnection(busName);

            await using var publisher = connection.BusClient.CreateSender(queueName);
            await publisher.SendMessageAsync(responseMsg);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError(nameof(QueueCreationStrategy), ex);
            
            throw new MessageDispatchException(
                $"Error sending command response to reply queue: {args.Message.ReplyTo}", ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private IMessage DeserializeReceivedMessage(ProcessMessageEventArgs args)
    {
        IMessage message = Context.DeserializeMessage(_dispatcher, args);
            
        // If this is a replay response to a prior command, set the CorrelationId and MessageId from the bus message.
        // This allows the message handler to correlate the replay to the original request.
        if (! string.IsNullOrWhiteSpace(args.Message.MessageId))
        {
            message.SetMessageId(args.Message.MessageId);
        }
        
        if (! string.IsNullOrWhiteSpace(args.Message.CorrelationId))
        {
            message.SetCorrelationId(args.Message.CorrelationId);
        }
        
        if (! string.IsNullOrWhiteSpace(args.Message.ContentType))
        {
            message.SetContentType(args.Message.ContentType);
        }

        if (! string.IsNullOrWhiteSpace(args.Message.ReplyTo))
        {
            message.SetReplyTo(args.Message.ReplyTo);
        }
            
        return message;
    }
    
    private ServiceBusMessage CreateResponseMessage(ProcessMessageEventArgs args, object response)
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
    
    private void LogReceivedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Received from {Queue} on {Bus}",
            message.GetType(),
            _queueEntity.EntityName,
            _queueEntity.BusName).WithProperties(
            LogProperty.ForName("QueueInfo", _queueEntity.GetLogProperties())
        );
            
        Logger.Log(log);
    }
    
    private void AddEntityDetailsToLog(MessageLog msgLog)
    {
        foreach ((string key, string? value) in _queueEntity.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
    }
    
    private void LogResponseMessage(IMessage response, string replyQueue)
    {
        Logger.LogDebug("Response {ResponseType} being sent to reply queue {ReplyQueue}", 
            response.GetType(), 
            replyQueue);
    }
    
    private static void AddResponseDetailsToLog(MessageLog msgLog, string replyQueue)
    {
        msgLog.AddLogDetail("ReplyQueue", replyQueue);
    }
    
    public async Task OnDispose()
    {
        if (_queueProcessor != null)
        {
            await _queueProcessor.DisposeAsync();
        }
    }
}