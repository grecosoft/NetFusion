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
        private readonly Guid _scopedRequestId = Guid.NewGuid();
        
        public override Task Enrich(IMessage message)
        {
            message.SetCorrelationId(Guid.NewGuid().ToString());    
            AddMessageProperty(message, "ScopedRequestId", _scopedRequestId);
            
            return base.Enrich(message);
        }
    }
}
