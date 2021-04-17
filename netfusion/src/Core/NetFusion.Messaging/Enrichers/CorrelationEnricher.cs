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
        // The MessageEnricherModule registers all enrichers with a scoped lifetime.
        // Therefore, the below guid value is unique per request for all published messages.
        private readonly Guid _scopedRequestId = Guid.NewGuid();
        
        public override Task EnrichAsync(IMessage message)
        {
            message.SetCorrelationId(_scopedRequestId.ToString());
            
            // This adds an unique value to identity each message.
            message.Attributes.SetGuidValue(
                AttributeExtensions.GetPluginScopedName("MessageId"),
                Guid.NewGuid(), false);

            return base.EnrichAsync(message);
        }
    }
}
