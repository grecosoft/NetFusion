using NetFusion.Domain.Messaging.Rules;
using WebApiHost.Messaging.Messages;

namespace WebApiHost.Messaging.Consumers.Rules
{
    public class IsHighImportance : MessageDispatchRule<ExampleRuleDomainEvent>
    {
        protected override bool IsMatch(ExampleRuleDomainEvent message)
        {
            return message.Value > 100;
        }
    }
}
