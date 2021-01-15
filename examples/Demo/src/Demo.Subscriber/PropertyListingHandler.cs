using System;
using Demo.Subscriber.Events;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace Demo.Subscriber
{
    public class PropertyListingHandler : IMessageConsumer
    {
        [TopicSubscription("netfusion-demo", "property-listing", "all-properties")]
        public void OnPropertyListed(NewListing domainEvent)
        {
            Console.WriteLine(domainEvent.ToIndentedJson());
        }
        
        [TopicSubscription("netfusion-demo", "property-listing", "expensive-properties")]
        public void OnExpensivePropertyListed(NewListing domainEvent)
        {
            Console.WriteLine(domainEvent.ToIndentedJson());
        }
    }
}