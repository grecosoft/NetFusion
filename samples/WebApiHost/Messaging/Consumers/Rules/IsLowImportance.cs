using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Rules;

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
