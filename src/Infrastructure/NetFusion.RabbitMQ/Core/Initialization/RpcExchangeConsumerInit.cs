using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Core.Rpc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Encapsulates the logic for subscribing the queues on which the application will monitor for RPC style messages.  
    /// When a message arrives on the queue, it is processed by dispatching the message to its associated message handler
    /// method.  After the message handler method is invoked, the response is sent back to the originating caller by 
    /// publishing the response on the corresponding reply queue specified by the caller in the message header.
    /// </summary>
    public class RpcExchangeConsumerInit : IBrokerInitializer
    {
        private ILogger _logger;
        private IMessagingModule _messagingModule;
        private MessageBrokerState _brokerState;
        private IConnectionManager _connMgr;
        private ISerializationManager _serializationMgr;

        public RpcExchangeConsumerInit(
           ILoggerFactory loggerFactory,
           IMessagingModule messagingModule,
           MessageBrokerState brokerState)
        {
            _logger = loggerFactory.CreateLogger<ExchangePublisherInit>();
            _messagingModule = messagingModule;
            _brokerState = brokerState;
            _connMgr = brokerState.ConnectionMgr;
            _serializationMgr = brokerState.SerializationMgr;
        }

        /// <summary>
        /// Creates a consumer called when a message arrives on one of the RPC based queues.  
        /// This consumer will dispatch the message to the corresponding message handler just
        /// as if were an in-process published message.
        /// </summary>
        public void BindConsumersToRpcQueues()
        {
            var rpcConsumers = _brokerState.Exchanges
                .Where(e => e.Settings.IsConsumerExchange)
                .SelectMany(e => e.Queues,
                    (e, q) => new {
                        BrokerName = e.BrokerName,
                        RpcQueue = q
                    }).ToList();

            foreach (var rpcConsumer in rpcConsumers)
            {
                IModel consumerChannel = _connMgr.CreateChannel(rpcConsumer.BrokerName);
                EventingBasicConsumer consumer = consumerChannel.GetBasicConsumer(rpcConsumer.RpcQueue);
                AttachRpcConsumerHandler(consumer);
            }
        }

        public void LogDetails(IDictionary<string, object> log)
        {

        }

        private void AttachRpcConsumerHandler(EventingBasicConsumer consumer)
        {
            consumer.Received += (sender, deliveryEvent) => {

                // Queue the message for processing so other messages can be received.  This allows .NET to allocate 
                // and manage the needed threads.  NOTE:  Since a worker thread is being used to process the message,
                // the channel must NOT be touched.  This is not needed since the RPC message is NoAck and the response
                // will be returned to the caller on another channel.
                ThreadPool.QueueUserWorkItem((stateInfo) => RpcConsumerReplyReceived(deliveryEvent));
            };
        }

        private void RpcConsumerReplyReceived(BasicDeliverEventArgs deliveryEvent)
        {
            ValidateRpcReply(deliveryEvent);

            // Determine the command type based on the type name stored in basic properties.  NOTE: a command type name is
            // just a value used to identify the type used to determine the C# class type on the receiving end corresponding
            // to the type on the caller's end.  They may or may not be the same physical C# type.
            string typeName = deliveryEvent.BasicProperties.Type;
            Type commandType = _brokerState.RpcTypes[typeName];

            // Dispatch the message the handler to obtain the result.
            MessageDispatchInfo dispatcher = _messagingModule.GetInProcessCommandDispatcher(commandType);
            IMessage message = _serializationMgr.Deserialize(commandType, deliveryEvent);
            object result = null;

            try
            {
                result = _messagingModule.InvokeDispatcherAsync(dispatcher, message).Result;

                // Publish the reply back to the publisher that made the request on the
                // queue specified by them in the message header on a new channel.
                PublishConsumerReply(result, deliveryEvent.BasicProperties);
            }
            catch(AggregateException ex)
            {
                PublishConsumerExceptionReply(ex.InnerException, deliveryEvent.BasicProperties);
            }
            catch(Exception ex)
            {
                PublishConsumerExceptionReply(ex, deliveryEvent.BasicProperties);
            }  
        }

        private void ValidateRpcReply(BasicDeliverEventArgs deleveryEvent)
        {
            var headers = deleveryEvent.BasicProperties.Headers ?? new Dictionary<string, object>();
            if (!headers.ContainsKey("broker-name"))
            {
                throw new BrokerException("The broker-name header value not present.");
            }

            string messageType = deleveryEvent.BasicProperties.Type;
            if (messageType.IsNullOrWhiteSpace())
            {
                throw new BrokerException(
                    "The basic properties of the received RPC request does not specify the message type.");
            }

            if (!_brokerState.RpcTypes.ContainsKey(messageType))
            {
                throw new BrokerException(
                    $"The type identified with the name: {messageType} could not be resolved to a corresponding .NET type.  " +
                    $"Make sure there is a command deriving from: {nameof(ICommand)} decorated with the following attribute " +
                    $"named: {nameof(RpcCommandAttribute)} specifying the type name.");
            }
        }

        private void PublishConsumerReply(object response, IBasicProperties requestProps)
        {
            byte[] replyBody = _serializationMgr.Serialize(response, requestProps.ContentType);
            PublishResponseToConsumer(replyBody, requestProps);
        }

        private void PublishResponseToConsumer(byte[] replyBody, IBasicProperties requestProps,
           IDictionary<string, object> headers = null)
        {
            headers = headers ?? new Dictionary<string, object>();
            string brokerName = requestProps.GetBrokerName();

            using (var replychannel = _connMgr.CreateChannel(brokerName))
            {
                IBasicProperties replyProps = replychannel.CreateBasicProperties();
                replyProps.ContentType = requestProps.ContentType;
                replyProps.CorrelationId = requestProps.CorrelationId;
                replyProps.Headers = headers;

                IBasicProperties replyprops = replychannel.CreateBasicProperties();

                replychannel.BasicPublish(exchange: "",
                    routingKey: requestProps.ReplyTo,
                    basicProperties: replyProps,
                    body: replyBody);
            }
        }

        private void PublishConsumerExceptionReply(Exception ex, IBasicProperties requestProps)
        {
            var dispatchEx = ex as MessageDispatchException;
            if (ex == null)
            {
                dispatchEx = new MessageDispatchException("Error dispatching RPC consumer", ex);
            }

            // Serialize the exception and make it the body of the message.  Indicate to the
            // publisher that the message body is the exception by setting message header.
            byte[] replyBody = _serializationMgr.Serialize(dispatchEx, requestProps.ContentType);
            var headers = new Dictionary<string, object> { { RpcClient.RPC_HEADER_EXCEPTION_INDICATOR, true } };

            PublishResponseToConsumer(replyBody, requestProps, headers);
        }
    }
}
