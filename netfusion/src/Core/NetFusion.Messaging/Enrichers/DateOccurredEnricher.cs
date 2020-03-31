using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Adds the current date and time to the message if not already present.
    /// </summary>
    public class DateOccurredEnricher : MessageEnricher
    {
        private readonly DateTime _scopedDateReceived = DateTime.UtcNow;
        
        public override Task Enrich(IMessage message)
        {
            AddMessageProperty(message, "DateOccurred", _scopedDateReceived);
            return base.Enrich(message);
        }
    }
}
