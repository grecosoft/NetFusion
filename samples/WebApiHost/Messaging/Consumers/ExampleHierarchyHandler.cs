using NetFusion.Messaging;
using WebApiHost.Messaging.Messages;

namespace WebApiHost.Messaging.Consumers
{
    public class ExampleHierarchyHandler : IMessageConsumer
    {
        [InProcessHandler]
        public void OnEvent([IncludeDerivedMessages]ExampleBaseDomainEvent evt)
        {
            evt.Attributes.Values.Message1 = "Base Handler Called";
        }

        [InProcessHandler]
        public void OnEvent(ExampleDerivedDomainEvent evt)
        {
            evt.Attributes.Values.Message2 = "Derived Handler Called";
        }
    }
}
