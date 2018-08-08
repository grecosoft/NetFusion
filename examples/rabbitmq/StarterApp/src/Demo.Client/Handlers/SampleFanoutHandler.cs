using System;
using Demo.Client.DomainEvents;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class SampleFanoutHandler : IMessageConsumer
    {
        [FanoutQueue("testBus", "TemperatureReading")]
        public void OnPriceDifference(TemperatureReading reading)
        {
            Console.WriteLine(reading.ToIndentedJson());
        }
    }
}