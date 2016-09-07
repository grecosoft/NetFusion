using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages.RabbitMQ
{
    [Serializable]
    public class ExampleTopicEvent : DomainEvent
    {
        public string Vin { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }

        public ExampleTopicEvent() { }

        public ExampleTopicEvent(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.Vin = car.Vin;
            this.Make = car.Make;
            this.Model = car.Model;
            this.Year = car.Year;

            this.SetRouteKey(car.Make, car.Model, car.Year);
        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
