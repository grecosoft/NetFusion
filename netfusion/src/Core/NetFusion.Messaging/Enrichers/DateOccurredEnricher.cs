using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Adds the current date and time to the message if not already present.
    /// </summary>
    public class DateOccurredEnricher : MessageEnricher
    {
        // The MessageEnricherModule registers all enrichers with a scoped lifetime.  Therefore,
        // the below date value will be the same for all published messages during a given request.
        private readonly DateTime _scopedDateReceived = DateTime.UtcNow;
        
        public override Task EnrichAsync(IMessage message)
        {
            message.SetUtcDateOccurred(_scopedDateReceived);
            return base.EnrichAsync(message);
        }
    }
}
