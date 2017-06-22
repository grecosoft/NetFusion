using ExampleApi.Models;
using NetFusion.Base;
using NetFusion.Domain.Messaging;
using System;

namespace ExampleApi.Messages
{
    [RpcCommand("TestBroker", "ExampleRpcConsumer", 
        nameof(ExampleRpcCommand), ContentType = SerializerTypes.MessagePack)]
    public class ExampleRpcCommand : Command<ExampleRpcResponse>
    {
        public DateTime CurrentDateTime { get; private set; }
        public string InputValue { get; private set; }
        public int DelayInMs { get; private set; }

        public ExampleRpcCommand()
        {
            
        }

        public ExampleRpcCommand(Car car)
        {
            var rand = new Random();
            this.DelayInMs = rand.Next(0, 500);

            this.CurrentDateTime = DateTime.UtcNow;
            this.InputValue = $"{car.Make + car.Model}";
        }
    }
}
