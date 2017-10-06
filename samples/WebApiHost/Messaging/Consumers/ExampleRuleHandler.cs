using NetFusion.Messaging;
using NetFusion.Messaging.Rules;
using NetFusion.Messaging.Types;
using WebApiHost.Messaging.Consumers.Rules;
using WebApiHost.Messaging.Messages;

namespace WebApiHost.Messaging.Consumers
{
    public class ExampleRuleHandler : IMessageConsumer
    {
        [InProcessHandler, ApplyDispatchRule(typeof(IsLowImportance))]
        public void OnEvent([IncludeDerivedMessages]DomainEvent evt)
        {
            evt.Attributes.Values.IsLowImportance = "Event is of low importance.";
        }

        [InProcessHandler, ApplyDispatchRule(typeof(IsHighImportance))]
        public void OnEvent(ExampleRuleDomainEvent evt)
        {
            evt.Attributes.Values.IsHighImportance = "Event is of high importance.";
        }
    }
}
