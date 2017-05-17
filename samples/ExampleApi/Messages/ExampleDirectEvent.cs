using ExampleApi.Models;
using NetFusion.Domain.Messaging;
using System;

namespace ExampleApi.Messages
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

            this.SetRouteKey(car.Make); 
        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
