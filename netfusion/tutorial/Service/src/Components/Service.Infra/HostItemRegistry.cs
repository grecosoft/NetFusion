using NetFusion.AMQP.Publisher;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.Infra
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