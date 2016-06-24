using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages
{
    public class ExampleFanoutEvent : DomainEvent
    {
        public string Make { get; set; }
        public string Model { get; set; }

        public ExampleFanoutEvent() { }

        public ExampleFanoutEvent(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.Make = car.Make;
            this.Model = car.Model;

            this.SetRouteKey(car.Make); 
        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
