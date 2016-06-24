using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages
{
    public class ExampleRpcCommand : Command<ExampleRpcResponse>
    {
        public DateTime CurrentDateTime { get; set; }
        public string InputValue { get; set; }

        public ExampleRpcCommand()
        {
            this.SetRouteKey("Hello");
        }

        public ExampleRpcCommand(Car car)
        {
            this.CurrentDateTime = DateTime.UtcNow;
            this.InputValue = $"{car.Make + car.Model}";
        }
    }
}
