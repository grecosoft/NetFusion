using ExampleApi.Messages;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Messaging;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;
using System;

namespace ConsumerHost.Services
{
    [Broker("TestBroker")]
    public class ExampleTopicService : IMessageConsumer
    {
        // This method will join the VW-GTI queue defined on the SampleTopicExchange
        // exchange.  Since it is joining an existing queue, it will join any other
        // enlisted subscribers and be called round-robin.  
        [JoinQueue("VW-GTI", "SampleTopicExchange")]
        public void OnVwGti(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnVwGti: {topicEvt.ToIndentedJson()}");

            topicEvt.SetAcknowledged();
        }

        // This event handler will join the VW-BLACK queue defined on the
        // SampleTopicExchange.  This handler is like the prior one, but the
        // associated queue has a more specific route-key pattern.  Both this
        // handler and the prior one will both be called since this handler 
        // has a more specific pattern to include the color of the car.
        [JoinQueue("VW-BLACK", "SampleTopicExchange")]
        public void OnBlackVw(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnBlackVw: {topicEvt.ToIndentedJson()}");

            topicEvt.SetAcknowledged();
        }

        // This event handler creates a new queue on SampleTopicExchange matching 
        // a specific route key.  This will not join an existing queue but will 
        // create a new queue specific for this host application.  If you start
        // another instance of this application, both will create their own queues
        // and therefore both be called.  This is more of a notification queue
        // since the message does not require an acknowledgment and the queue
        // will be deleted when the host application disconnects.
        [AddQueue("SampleTopicExchange", RouteKey = "VW.JETTA.*.*",
            IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
        public void OnVwJetta(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnVwJetta: {topicEvt.ToIndentedJson()}");
        }

        // This adds a queue with a more specific pattern.  This queue is configured 
        // to require message acknowlegement.  Also auto-delete is not set so the queue
        // will remain after the host application disconnects.  This would be the setup
        // for important messages that are specific to a given application host.
        [AddQueue("VW-PASSAT-SILVER", "SampleTopicExchange", RouteKey = "VW.PASSAT.*.SILVER",
            IsAutoDelete = false, IsNoAck = false, IsExclusive = false)]
        public void OnSilverVwPassat(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnSilverVwPassat: {topicEvt.ToIndentedJson()}");

            topicEvt.SetAcknowledged();
        }

        // This example joins a queue defined by the publisher where the route-key
        // values are stored in the configuration and not specified in code.
        [JoinQueue("AUDI", "SampleTopicExchange")]
        public void OnAudi(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnAudi: {topicEvt.ToIndentedJson()}");

            topicEvt.SetAcknowledged();
        }
    }
}
