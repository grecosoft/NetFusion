﻿using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Base implementation of a message enricher that can be derived.
    /// </summary>
    public abstract class MessageEnricher : IMessageEnricher
    {
        /// <summary>
        /// Overridden by derived class to enrich the message.
        /// </summary>
        /// <param name="message">Reference to the message to enrich.</param>
        /// <returns>Future Result</returns>
        public virtual Task Enrich(IMessage message)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Utility method that will add a key/value to the message if not already
        /// present.  The type of the derived enricher is used as the context.
        /// </summary>
        /// <param name="message">The message to add key/value.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        protected void AddMessageProperty(IMessage message, string name, object value)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (name == null) throw new ArgumentNullException(nameof(name));

            message.Attributes.SetValue(name, value, typeof(MessagingContext), overrideIfPresent: false);
        }
    }
}
