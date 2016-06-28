using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages.RabbitMQ
{
    public class ExampleWorkQueueEvent : DomainEvent
    {
        public string Vin { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }

        public ExampleWorkQueueEvent() { }

        public ExampleWorkQueueEvent(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.Make = car.Make;
            this.Model = car.Model;
            this.Year = car.Year;

            this.SetRouteKey(car.Make.InSet("VW", "BMW") ? "ProcessSale" : "ProcessService");
        }

        public DateTime CurrentDateTime { get; private set; }
    }
}
