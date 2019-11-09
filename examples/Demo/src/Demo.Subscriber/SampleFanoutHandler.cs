using System;
using Demo.Domain.Events;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Subscriber
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
