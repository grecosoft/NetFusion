using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.RabbitMQ.Models;
using System;

namespace RefArch.Api.RabitMQ.Messages
{
    public class ExampleDirectEvent : DomainEvent
    {
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public string Color { get; private set; }

        public ExampleDirectEvent() { }

        public ExampleDirectEvent(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.Make = car.Make;
            this.Model = car.Model;
            this.Year = car.Year;
            this.Color = car.Color;

            this.SetRouteKey(car.Make); // TODO:  move this out of the RabbitMQ project??? So client don't need ref?
        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
