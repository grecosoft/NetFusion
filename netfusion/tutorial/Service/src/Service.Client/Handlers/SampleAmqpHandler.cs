using System;
using NetFusion.AMQP.Plugin;
using NetFusion.AMQP.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.Client.Handlers
{
    [Host("claims-bus")]
    public class SampleAmqpHandler : IMessageConsumer
    {
        private readonly IConnectionModule _connectionModule;

        public SampleAmqpHandler(IConnectionModule connectionModule)
        {
            _connectionModule = connectionModule;
        }
        
        [Queue("claim-submissions")]
        public void OnClaimSubmission(CreateClaimSubmission command)
        {
           Console.WriteLine(command.ToIndentedJson());
        }

        //[Topic("claim-status-notification", "all-notifications")]
        public void OnClaimStatus(ClaimStatusUpdated domainEvent)
        {
            Console.WriteLine(domainEvent.ToIndentedJson());
        }
    }
}