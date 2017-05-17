using ExampleApi.Models;
using NetFusion.Domain.Messaging;
using System;

namespace ExampleApi.Messages
{
    public class ExampleFanoutEvent : DomainEvent
    {
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public string Color { get; private set; }

        public ExampleFanoutEvent() { }

        public ExampleFanoutEvent(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.Make = car.Make;
            this.Model = car.Model;
            this.Year = car.Year;
            this.Color = car.Color;
        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
