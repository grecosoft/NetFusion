using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Rules;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Consumers;
using RefArch.Api.Messages;
using System;

namespace RefArch.Subscriber.Services
{
    [Broker("TestBroker")]
    public class ExampleTopicService : IMessageConsumer
    {
        // This method will join the Chevy queue defined on the SampleTopicExchange
        // exchange.  Since it is joining an existing queue, it will join any other
        // enlisted subscribers and be called round-robin.  
        [JoinQueue("Chevy", "SampleTopicExchange")]
        public void OnChevy(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnChevy: {topicEvt.ToIndentedJson()}");

            topicEvt.SetAcknowledged();
        }

        // This event handler will join the Chevy-Vette queue defined on the
        // SampleTopicExchange.  This handler is like the prior one, but the
        // associated queue has a more specific route-key pattern.  Both this
        // handler and the prior one will both be called since this handler 
        // has a more specific pattern to include the model of the car.
        [JoinQueue("Chevy-Vette", "SampleTopicExchange")]
        public void OnChevyVette(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnChevyVette: {topicEvt.ToIndentedJson()}");

            topicEvt.SetAcknowledged();
        }

        // This event handler joins the Ford queue defined on the same
        // exchange as the prior two event handlers.
        [JoinQueue("Ford", "SampleTopicExchange")]
        public void OnFord(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnFord: {topicEvt.ToIndentedJson()}");

            topicEvt.SetAcknowledged();
        }

        // This event handler creates a new queue on SampleTopicExchange 
        // matching any route key.  However, this event handler has a 
        // dispatch-role specified to only be called if the Make of the
        // car is <= three characters.  The event is still delivered but
        // just not passed to this handler.  If there are a large number
        // of events, it is best to create a dedicated queue on the 
        // exchange.
        [ApplyDispatchRule(typeof(ShortMakeRule))]
        [AddQueue("SampleTopicExchange", RouteKey = "#",
            IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
        public void OnSortMakeName(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnSortMakeNam: {topicEvt.ToIndentedJson()}");
        }

        // This adds a queue with a more specific pattern.  Since it 
        // creating a new queue, it will be called in addition to the
        // event handler that is specified for the Ford queue.
        [AddQueue("SampleTopicExchange", RouteKey = "Ford.Mustang.*",
            IsAutoDelete = true, IsNoAck = true, IsExclusive = true)]
        public void OnFordMustang(ExampleTopicEvent topicEvt)
        {
            Console.WriteLine($"Handler: OnFordMustang: {topicEvt.ToIndentedJson()}");
        }
    }
}
