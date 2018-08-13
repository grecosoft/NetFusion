using System;
using Demo.Client.DomainEvents;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class SampleDirectHandler : IMessageConsumer
    {
        [DirectQueue("testBus", "NorthEast", "RealEstate",
            "CT", "NY", "NH", "ME")]
        public void NorthEast(PropertySold propertySold)
        {
            Console.WriteLine($"Exchange=>RealEstate; Queue=>NorthEast; Route-Key: {propertySold.GetRouteKey()}");
        }

        [DirectQueue("testBus", "SouthEast", "RealEstate",
            "NC", "SC", "FL")]
        public void SouthEast(PropertySold propertySold)
        {
            Console.WriteLine($"Exchange=>RealEstate; Queue=>SouthEast; Route-Key: {propertySold.GetRouteKey()}");
        }
    }
}