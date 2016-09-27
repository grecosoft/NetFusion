using NetFusion.Messaging.Rules;
using RefArch.Api.Messaging.Messages;

namespace RefArch.Services.Messaging.Rules
{
    public class IsHighImportance : MessageDispatchRule<ExampleRuleDomainEvent>
    {
        protected override bool IsMatch(ExampleRuleDomainEvent message)
        {
            return message.Value > 100;
        }
    }
}
