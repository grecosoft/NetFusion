using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Serialization;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages.RabbitMQ
{
    [RpcCommand("TestBroker", "ExampleRpcConsumer", 
        ExternalTypeName = "Example_Command", ContentType = SerializerTypes.Binary)]
    public class ExampleRpcCommand : Command<ExampleRpcResponse>
    {
        public DateTime CurrentDateTime { get; private set; }
        public string InputValue { get; private set; }

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
