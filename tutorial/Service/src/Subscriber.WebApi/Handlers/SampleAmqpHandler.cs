using System;
using NetFusion.AMQP.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using Subscriber.WebApi.Commands;
using Subscriber.WebApi.Events;

namespace Subscriber.WebApi.Handlers
{
    [Host("claims-bus")]
    public class SampleAmqpHandler : IMessageConsumer
    {
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