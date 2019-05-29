using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Service.Client.Events;

namespace Service.Client.Handlers
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