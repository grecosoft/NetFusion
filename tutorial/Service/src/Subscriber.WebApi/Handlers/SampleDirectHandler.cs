using System;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Subscriber.WebApi.Events;

namespace Subscriber.WebApi.Handlers
{
    public class SampleDirectHandler : IMessageConsumer
    {
        [DirectQueue("testBus", "NorthEast", "RealEstate",
            "CT", "NY", "NH", "ME", IsNonRoutedSaved = true, IsUnacknowledgedSaved = true)]
        public void NorthEast(PropertySold propertySold)
        {
            Console.WriteLine(propertySold.ToIndentedJson());
        }

        [DirectQueue("testBus", "SouthEast", "RealEstate",
            "NC", "SC", "FL", IsNonRoutedSaved = true)]
        public void SouthEast(PropertySold propertySold)
        {
            Console.WriteLine(propertySold.ToIndentedJson());
        }
    }
}