using System;
using Demo.Subscriber.Commands;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace Demo.Subscriber
{
    public class MembershipProcessing : IMessageConsumer
    {
        [QueueSubscription("netfusion-demo", "card-issuance")]
        public void OnNewMembership(IssueMembershipCard command)
        {
            Console.WriteLine(command.ToIndentedJson());
        }
    }
}