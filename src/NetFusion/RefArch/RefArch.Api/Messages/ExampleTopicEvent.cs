using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages
{
    public class ExampleTopicEvent : DomainEvent
    {
        public string Vin { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }

        public ExampleTopicEvent() { }

        public ExampleTopicEvent(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.Vin = car.Vin;
            this.Make = car.Make;
            this.Model = car.Model;
            this.Year = car.Year;

            this.SetRouteKey(car.Make, car.Year, car.Year);
        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
