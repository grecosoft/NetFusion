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
        //The MessageEnricherModule registers all enrichers with a scoped lifetime,
        // So the below guid value is unique per request.
        private readonly Guid _scopedCorrelationId = Guid.NewGuid();
        
        public override Task Enrich(IMessage message)
        {
            message.SetCorrelationId(_scopedCorrelationId.ToString());            
            return base.Enrich(message);
        }
    }
}
