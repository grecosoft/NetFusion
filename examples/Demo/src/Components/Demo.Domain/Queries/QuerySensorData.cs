using System;
using NetFusion.Messaging.Types;

namespace Demo.Domain.Queries
{
    public class QuerySensorData : Query<SensorReading>,
        ITimestamp
    {
        public string DeviceId { get; }
        
        public QuerySensorData(string deviceId)
        {
            DeviceId = deviceId;
        }
        
        public DateTime CurrentDate { get; set; }
        public string Message { get; set; }
    }
}