using Demo.Domain.Commands;
using Demo.Domain.Events;
using NetFusion.AMQP.Publisher;

namespace Demo.Infra
{
    public class HostItemRegistry : HostRegistryBase
    {
        public override string Namespace { get; } = "claims-bus";

        public override void OnRegister()
        {
            AddQueue<CreateClaimSubmission>("claim-submissions");
            AddTopic<ClaimStatusUpdated>("claim-status-notification");
        }
    }
}
