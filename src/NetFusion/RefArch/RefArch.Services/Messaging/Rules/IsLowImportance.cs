using NetFusion.Messaging;
using NetFusion.Messaging.Rules;

namespace RefArch.Services.Messaging.Rules
{
    public class IsLowImportance : MessageDispatchRule<DomainEvent>
    {
        protected override bool IsMatch(DomainEvent message)
        {
            return message.AttributesOld.Keys.Contains("__low__");
        }
    }
}
