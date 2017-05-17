using ExampleApi.Messages;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;
using System;

namespace ConsumerHost.Services
{
    [Broker("TestBroker")]
    public class FanoutExchangeService : IMessageConsumer
    {
        [AddFanoutQueue("HighImportanceExchange")]
        public void OnHighImportance(ExampleFanoutEvent fanoutEvt)
        {
            Console.WriteLine($"Handler: OnHighImportance: { fanoutEvt.ToIndentedJson()}");
        }

        [AddFanoutQueue("LowImportanceExchange")]
        public void OnLowImportance(ExampleFanoutEvent fanoutEvt)
        {
            Console.WriteLine($"Handler: OnLowImportance: {fanoutEvt.ToIndentedJson()}");
        }
    }
}
