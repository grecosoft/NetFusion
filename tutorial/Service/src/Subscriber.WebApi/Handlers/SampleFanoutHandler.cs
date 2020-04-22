using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Subscriber.WebApi.Events;

namespace Subscriber.WebApi.Handlers
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