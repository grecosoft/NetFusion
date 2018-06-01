﻿using NetFusion.Messaging.Types;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Types implementing this interface are registered within the application's
    /// bootstrap configuration and called for each published message.  The role
    /// of an enricher is to added key/value pairs to the message.
    /// </summary>
    public interface IMessageEnricher
    {
        /// <summary>
        /// Implementation should add a key/value part to the message if it is
        /// not already present.
        /// </summary>
        /// <param name="message">The message to enrich.</param>
        /// <returns>Future Result</returns>
        Task Enrich(IMessage message);
    }
}
