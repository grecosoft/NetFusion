using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging.Core;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging.Config
{
    /// <summary>
    /// Messaging specific plug-in configurations.
    /// </summary>
    public class MessagingConfig : IContainerConfig
    {
        // The list message publishers that can handle message delivery.  
        // The default publisher dispatches messages to in-process event handlers.
        private List<Type> _messagePublisherTypes = new List<Type> { typeof(InProcessMessagePublisher) };

        /// <summary>
        /// The publishers that will be called to deliver published messages.
        /// </summary>
        public Type[] PublisherTypes { get { return _messagePublisherTypes.ToArray(); } }

        /// <summary>
        /// Clears any registered default message publishers.
        /// </summary>
        public void ClearMessagePublishers()
        {
            _messagePublisherTypes.Clear();
        }

        /// <summary>
        /// Adds message publisher to be executed when a message is published.  
        /// The order specified is the order in which they are executed.
        /// </summary>
        /// <typeparam name="TPublisher"></typeparam>
        public void AddMessagePublisherType<TPublisher>() where TPublisher: IMessagePublisher
        {
            Type publisherType = typeof(TPublisher);
            if (_messagePublisherTypes.Contains(publisherType))
            {
                throw new ContainerException(
                    $"The message publisher of type: {publisherType} is already registered.");
            }

            _messagePublisherTypes.Add(publisherType);
        }
    }
}
