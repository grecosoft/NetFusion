using System;
using Demo.Client.DomainEvents;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class SampleDirectHandler : IMessageConsumer
    {
        [DirectQueue("NorthEast", "RealEstate",
            "CT", "NY", "NH", "ME", BusName = "testBus")]
        public void NorthEast(PropertySold propertySold)
        {
            Console.WriteLine(propertySold.ToIndentedJson());
        }

        [DirectQueue("SouthEast", "RealEstate",
            "NC", "SC", "FL", BusName = "testBus")]
        public void SouthEast(PropertySold propertySold)
        {
            Console.WriteLine(propertySold.ToIndentedJson());
        }
    }
}