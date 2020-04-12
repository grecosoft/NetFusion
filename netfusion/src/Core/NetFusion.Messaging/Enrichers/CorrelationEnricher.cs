using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Adds a GUID value as the message's correlation identifier if not present.
    /// </summary>
    public class CorrelationEnricher : MessageEnricher
    {
        // The MessageEnricherModule registers all enrichers with a scoped lifetime,
        // So the below guid value is unique per request.
        private readonly Guid _scopedRequestId = Guid.NewGuid();
        
        public override Task Enrich(IMessage message)
        {
            message.SetCorrelationId(Guid.NewGuid().ToString());
            
            // All messages published within the same lifetime scope
            // wll have the same ScopedRequestId.
            message.Attributes.SetGuidValue(
                AttributeExtensions.GetPluginScopedName("ScopedRequestId"),
                _scopedRequestId, false);

            return base.Enrich(message);
        }
    }
}
