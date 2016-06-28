using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;
using RefArch.Api.Messages.RabbitMQ;
using System;

namespace RefArch.Subscriber.Services
{
    [Broker("TestBroker")]
    public class FanoutExchangeService : IMessageConsumer
    {
        [AddFanoutQueue("SampleFanoutExchange->GermanCars")]
        public void OnGermanCars(ExampleFanoutEvent fanoutEvt)
        {
            Console.WriteLine($"Handler: OnGermanCars: { fanoutEvt.ToIndentedJson()}");
        }

        [AddFanoutQueue("SampleFanoutExchange->AmericanCars")]
        public void OnAmericanCars(ExampleFanoutEvent fanoutEvt)
        {
            Console.WriteLine($"Handler: OnAmericanCars: {fanoutEvt.ToIndentedJson()}");
        }
    }
}
