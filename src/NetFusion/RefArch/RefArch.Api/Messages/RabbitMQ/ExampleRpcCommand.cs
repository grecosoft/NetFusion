using NetFusion.Messaging;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Serialization;
using RefArch.Api.Models;
using System;

namespace RefArch.Api.Messages.RabbitMQ
{
    [RpcCommand("TestBroker", "ExampleRpcConsumer", 
        nameof(ExampleRpcCommand), ContentType = SerializerTypes.Json)]
    public class ExampleRpcCommand : Command<ExampleRpcResponse>
    {
        public DateTime CurrentDateTime { get; private set; }
        public string InputValue { get; private set; }
        public int DelayInMs { get; private set; }
        public string TestValue { get; private set; }

        public ExampleRpcCommand()
        {
            
        }

        public ExampleRpcCommand(Car car)
        {
            var rand = new Random();
            this.DelayInMs = 0; // rand.Next(0, 200);
            this.TestValue = car.TestValue;

            this.CurrentDateTime = DateTime.UtcNow;
            this.InputValue = $"{car.Make + car.Model}";
        }
    }
}
