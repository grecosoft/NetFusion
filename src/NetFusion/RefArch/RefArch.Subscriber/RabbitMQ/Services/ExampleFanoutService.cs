using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;
using RefArch.Api.RabitMQ.Messages;
using System;

namespace RefArch.Subscriber.RabbitMQ.Services
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
