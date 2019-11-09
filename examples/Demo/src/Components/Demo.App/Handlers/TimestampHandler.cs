using System;
using Demo.Domain.Queries;
using NetFusion.Messaging;

namespace Demo.App.Handlers
{
    public class TimestampHandler : IQueryConsumer
    {
        [InProcessHandler]
        public SensorReading When(QuerySensorData query)
        {
            Console.WriteLine($"Reading Device: {query.DeviceId}");
            
            return new SensorReading
            {
                MinValue = 10,
                MaxValue = 130
            };
        }
    }
}