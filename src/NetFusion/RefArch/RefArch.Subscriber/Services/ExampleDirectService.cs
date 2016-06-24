using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Consumers;
using RefArch.Api.Messages;
using System;

namespace RefArch.Subscriber.Services
{
    [Broker("TestBroker")]
    public class ExampleDirectService : IMessageConsumer
    {
        // This method will join to the 2015-2016-Cars queue defined on the
        // ExampleDirectExchange.  Since this handler is joining the queue,
        // it will be called round-robin with other subscribed clients.
        [JoinQueue("2015-2016-Cars", "SampleDirectExchange")]
        public void OnModelYear(ExampleDirectEvent directEvt)
        {
            Console.WriteLine("Handler: OnModelYear[2015-2016-Cars]");
            Console.WriteLine(directEvt.ToIndentedJson());

            directEvt.SetAcknowledged();
        }

        // This method will join to the UsedCars queue defined on the
        // ExampleDirectExchange.  Since this handler is joining the queue,
        // it will be called round-robin with other subscribed clients.
        [JoinQueue("UsedCars", "SampleDirectExchange")]
        public void OnUsedCars(ExampleDirectEvent directEvt)
        {
            Console.WriteLine("Handler: OnUsedCars[UsedCars]");
            Console.WriteLine(directEvt.ToIndentedJson());

            directEvt.SetAcknowledged();
        }
    }
}
