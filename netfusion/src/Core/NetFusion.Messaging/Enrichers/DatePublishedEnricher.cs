using NetFusion.Messaging.Types;
using System;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Adds the current date and time to the message if not already present.
    /// </summary>
    public class DateReceivedEnricher : MessageEnricher
    {
        private readonly DateTime _scopedDateReceived = DateTime.UtcNow;
        
        public override Task Enrich(IMessage message)
        {
            AddMessageProperty(message, "DatePublished", _scopedDateReceived);
            return base.Enrich(message);
        }
    }
}
