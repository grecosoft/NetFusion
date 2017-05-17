using NetFusion.Domain.Messaging;
using NetFusion.Domain.Messaging.Rules;

namespace WebApiHost.Messaging.Consumers.Rules
{
    public class IsLowImportance : MessageDispatchRule<DomainEvent>
    {
        protected override bool IsMatch(DomainEvent message)
        {
            return message.Attributes.Contains("__low__");
        }
    }
}
