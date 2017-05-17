using ExampleApi.Messages;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Messaging;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;
using System;

namespace ConsumerHost.Services
{
    [Broker("TestBroker")]
    public class ExampleDirectService : IMessageConsumer
    {
        // This method will join to the 2015-2016-Cars queue defined on the
        // ExampleDirectExchange.  Since this handler is joining the queue,
        // it will be called round-robin with other subscribed clients.
        [JoinQueue("GENERAL-MOTORS", "SampleDirectExchange")]
        public void OnGeneralMotors(ExampleDirectEvent directEvt)
        {
            Console.WriteLine("Handler: OnGeneralMotors");
            Console.WriteLine(directEvt.ToIndentedJson());

            directEvt.SetAcknowledged();
        }

        // This method will join to the UsedCars queue defined on the
        // ExampleDirectExchange.  Since this handler is joining the queue,
        // it will be called round-robin with other subscribed clients.
        [JoinQueue("VOLKSWAGEN", "SampleDirectExchange")]
        public void OnVolkswagen(ExampleDirectEvent directEvt)
        {
            Console.WriteLine("Handler: OnVolkswagen");
            Console.WriteLine(directEvt.ToIndentedJson());

            directEvt.SetAcknowledged();
        }
    }
}
