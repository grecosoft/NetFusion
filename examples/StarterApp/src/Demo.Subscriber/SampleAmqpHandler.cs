using System;
using NetFusion.AMQP.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using Demo.Subscriber.Commands;
using Demo.Subscriber.Events;

namespace Demo.Subscriber
{
    [Host("claims-bus")]
    public class SampleAmqpHandler : IMessageConsumer
    {

        [Queue("claim-submissions")]
        public void OnClaimSubmission(CreateClaimSubmission command)
        {
            Console.WriteLine(command.ToIndentedJson());
        }

//        [Topic("claim-status-notification", "all-notifications")]
//        public void OnClaimStatus(ClaimStatusUpdated domainEvent)
//        {
//            Console.WriteLine(domainEvent.ToIndentedJson());
//        }
    }
}
