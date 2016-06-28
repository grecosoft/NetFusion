using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages.RabbitMQ
{
    public class ExampleDirectEvent : DomainEvent
    {
        public string Vin { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }

        public ExampleDirectEvent() { }

        public ExampleDirectEvent(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.Vin = car.Vin;
            this.Make = car.Make;
            this.Model = car.Model;
            this.Year = car.Year;

            this.SetRouteKey(car.Year); // TODO:  move this out of the RabbitMQ project??? So client don't need ref?

            if (car.Year < 2015)
            {
                this.SetRouteKey("UsedModel");
            }

        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
