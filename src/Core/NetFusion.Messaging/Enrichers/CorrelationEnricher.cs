using NetFusion.Messaging.Types;
using System;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Adds a GUID value as the message's correlation identifier if not present.
    /// </summary>
    public class CorrelationEnricher : MessageEnricher
    {
        public override Task Enrich(IMessage message)
        {
            message.SetCorrelationId(Guid.NewGuid().ToString());            
            return base.Enrich(message);
        }
    }
}
