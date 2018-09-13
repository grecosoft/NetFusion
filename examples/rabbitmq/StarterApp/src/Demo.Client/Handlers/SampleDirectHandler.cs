using System;
using Demo.Domain.Events;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class SampleDirectHandler : IMessageConsumer
    {
        [DirectQueue("testBus", "NorthEast", "RealEstate",
            "CT", "NY", "NH", "ME")]
        public void NorthEast(PropertySold propertySold)
        {
            Console.WriteLine(propertySold.ToIndentedJson());
        }

        [DirectQueue("testBus", "SouthEast", "RealEstate",
            "NC", "SC", "FL")]
        public void SouthEast(PropertySold propertySold)
        {
            Console.WriteLine(propertySold.ToIndentedJson());
        }
    }
}
