using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using NetFusion.Azure.ServiceBus.Plugin;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Base.Serialization;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Namespaces
{
    /// <summary>
    /// Content set on strategies implementing the IRequiresContext interface.
    /// Provides access to common modules, services, and utility methods.
    /// </summary>
    public class NamespaceContext 
    {
        public INamespaceModule NamespaceModule { get; }
        public IMessageDispatchModule DispatchModule { get; }

        public NamespaceContext(INamespaceModule namespaceModule, IMessageDispatchModule dispatchModule)
        {
            NamespaceModule = namespaceModule ?? throw new ArgumentNullException(nameof(namespaceModule));
            DispatchModule = dispatchModule ?? throw new ArgumentNullException(nameof(dispatchModule));
        }
        
        /// <summary>
        /// Reference to logger for writing logs specific to the strategy.
        /// </summary>
        public ILogger Logger { get; internal set; }
        
        /// <summary>
        /// Reference to the common serialization manager.
        /// </summary>
        public ISerializationManager Serialization { get; internal set; }
        
        /// <summary>
        /// Deserializes a messages received by a queue or topic subscription.
        /// </summary>
        /// <param name="dispatchInfo">The information associated with the local message handler.</param>
        /// <param name="messageEventArgs">Information about the message received.</param>
        /// <returns>Returns messaged deserialized into the message type.</returns>
        public IMessage DeserializeMessage(MessageDispatchInfo dispatchInfo, 
            ProcessMessageEventArgs messageEventArgs)
        {
            if (dispatchInfo == null) throw new ArgumentNullException(nameof(dispatchInfo));
            if (messageEventArgs == null) throw new ArgumentNullException(nameof(messageEventArgs));

            BinaryData messageData = messageEventArgs.Message.Body;
            string contentType = messageEventArgs.Message.ContentType;

            return (IMessage)Serialization.Deserialize(contentType, dispatchInfo.MessageType, messageData.ToArray());
        }
        
        /// <summary>
        /// Parses the colon separated ReplyTo value into the namespace and queue name.
        /// </summary>
        /// <param name="args">The information about the message being processed.</param>
        /// <param name="namespaceName">If found, the namespace to which the reply message is to be sent.</param>
        /// <param name="queueName">If found, the queue to which the reply message is to be sent.</param>
        /// <returns>True if the ReplyTo is correctly formatted.</returns>
        public static bool TryParseReplyTo(ProcessMessageEventArgs args, 
            out string namespaceName, out string queueName)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            
            var parts = args.Message.ReplyTo?.Split(":");
            if (parts == null || parts.Length != 2)
            {
                namespaceName = queueName = null;
                return false;
            }

            namespaceName = parts[0]; 
            queueName = parts[1];
            return true;
        }

        public void LogSubscription(EntitySubscription subscription)
        {
            Logger.LogDebug("Message {MessageType} received on {EntitySubscription}", 
                subscription.DispatchInfo.MessageType, 
                subscription);
        }

        /// <summary>
        /// Common method for logging message processing errors.
        /// </summary>
        /// <param name="eventArgs">Error information about the message being processed.</param>
        public void LogProcessError(ProcessErrorEventArgs eventArgs)
        {
            Logger.Log(LogLevel.Error, eventArgs.Exception, 
                "Error of source {ErrorSource} for the entity {EntityPath} within namespace {Namespace} received.", 
                eventArgs.ErrorSource,
                eventArgs.EntityPath, 
                eventArgs.FullyQualifiedNamespace);
        }
    }
}