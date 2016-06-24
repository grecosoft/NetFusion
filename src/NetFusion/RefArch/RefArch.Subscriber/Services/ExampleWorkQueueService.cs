using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Consumers;
using RefArch.Api.Messages;
using System;

namespace RefArch.Subscriber.Services
{
    [Broker("TestBroker")]
    public class ExampleWorkQueueService : IMessageConsumer
    {
        [JoinQueue("ProcessSale")]
        public void OnProcessSale(ExampleWorkQueueEvent workQueueEvent)
        {
            Console.WriteLine($"Handler: OnProcessSale: {workQueueEvent.ToIndentedJson()}");

            workQueueEvent.SetAcknowledged();
        }

        [JoinQueue("ProcessService")]
        public void OnProcessService(ExampleWorkQueueEvent workQueueEvent)
        {
            Console.WriteLine($"Handler: OnProcessService: {workQueueEvent.ToIndentedJson()}");

            workQueueEvent.SetAcknowledged();
        }
    }
}
