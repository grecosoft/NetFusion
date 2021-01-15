using System;
using Demo.Subscriber.Events;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace Demo.Subscriber
{
    public class SubmissionHandler : IMessageConsumer
    {
        [TopicSubscription("netfusion-demo", "submissions", "n", IsFanout = true)]
        public void OnSubmission(NewSubmission domainEvent)
        {
            Console.WriteLine(domainEvent.ToIndentedJson());
        }    
    }
}