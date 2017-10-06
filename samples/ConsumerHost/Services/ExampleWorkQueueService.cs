using ExampleApi.Messages;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Consumers;
using System;

namespace ConsumerHost.Services
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
