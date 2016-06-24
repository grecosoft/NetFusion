using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages
{
    public class ExampleWorkQueueEvent : DomainEvent
    {
        public string Vin { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }

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
