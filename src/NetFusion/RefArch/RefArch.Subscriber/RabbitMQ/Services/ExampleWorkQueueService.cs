using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Consumers;
using RefArch.Api.RabitMQ.Messages;
using System;

namespace RefArch.Subscriber.RabbitMQ.Services
{
    [Broker("TestBroker")]
    public class ExampleWorkQueueService : IMessageConsumer
    {
        [JoinQueue("PROCESS_SALE")]
        public void OnProcessSale(ExampleWorkQueueEvent workQueueEvent)
        {
            Console.WriteLine($"Handler: OnProcessSale: {workQueueEvent.ToIndentedJson()}");

            workQueueEvent.SetAcknowledged();
        }

        [JoinQueue("PROCESS_SERVICE")]
        public void OnProcessService(ExampleWorkQueueEvent workQueueEvent)
        {
            Console.WriteLine($"Handler: OnProcessService: {workQueueEvent.ToIndentedJson()}");

            workQueueEvent.SetAcknowledged();
        }
    }
}
