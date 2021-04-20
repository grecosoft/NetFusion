using System;
using NetFusion.Base.Exceptions;
using NetFusion.Messaging.Enrichers;

namespace NetFusion.Messaging.Exceptions
{
    /// <summary>
    /// Contains information about a message enricher that throws and exception when applied.
    /// </summary>
    public class EnricherException : NetFusionException
    {
        public EnricherException() { }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        /// <param name="messageEnricher">The associated enricher that raised exception.</param>
        /// <param name="aggregateException">The aggregate exception associated with task.</param>
        public EnricherException(string message, IMessageEnricher messageEnricher, 
            AggregateException aggregateException)
            : base(message, aggregateException)
        {
            if (messageEnricher == null) throw new NullReferenceException(nameof(messageEnricher));

            Details["Enricher"] = messageEnricher.GetType().FullName; 
        }
    }
}
