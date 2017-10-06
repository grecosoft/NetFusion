using NetFusion.Messaging.Types;
using WebApiHost.Messaging.Models;

namespace WebApiHost.Messaging.Messages
{
    public class ExampleRuleDomainEvent : DomainEvent
    {
        public int Value { get; }

        public ExampleRuleDomainEvent(MessageRuleInfo info) {
            this.Value = info.Value;

            if (this.Value == 50)
            {
                this.Attributes.SetValue("__low__", "");
            }
        }
    }
}
