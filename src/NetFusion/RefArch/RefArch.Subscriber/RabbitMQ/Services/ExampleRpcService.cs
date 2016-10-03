using NetFusion.Messaging;
using RefArch.Api.RabitMQ.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RefArch.Subscriber.RabbitMQ.Services
{
    public class ExampleRpcService : IMessageConsumer
    {
        [InProcessHandler]
        public ExampleRpcResponse OnRpcMessage(ExampleRpcCommand rpcCommand)
        {
            Console.WriteLine($"Delay: {rpcCommand.DelayInMs} TestValue: {rpcCommand.TestValue}");

            //await Task.Run(() =>
            //{
                throw new InvalidOperationException("TEST");
                Thread.Sleep(rpcCommand.DelayInMs);
            //});

            return new ExampleRpcResponse
            {
                ResponseTestValue = rpcCommand.TestValue
            };
        }
    }
}
