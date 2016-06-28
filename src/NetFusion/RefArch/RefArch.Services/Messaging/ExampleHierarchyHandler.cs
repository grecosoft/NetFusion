using NetFusion.Messaging;
using RefArch.Api.Messages;

namespace RefArch.Services.Messaging
{
    public class ExampleHierarchyHandler : IMessageConsumer
    {
        [InProcessHandler]
        public void OnEvent([IncludeDerivedMessages]ExampleBaseDomainEvent evt)
        {
            evt.Attributes["Message1"] = "Base Handler Called";
        }

        [InProcessHandler]
        public void OnEvent(ExampleDerivedDomainEvent evt)
        {
            evt.Attributes["Message"] = "Derived Handler Called";
        }
    }
}
