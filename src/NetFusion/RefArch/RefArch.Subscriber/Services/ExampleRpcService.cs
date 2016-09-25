using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using RefArch.Api.Messages.RabbitMQ;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RefArch.Subscriber.Services
{
    public class ExampleRpcService : IMessageConsumer
    {
        [InProcessHandler]
        public async Task<ExampleRpcResponse> OnRpcMessage(ExampleRpcCommand rpcCommand)
        {
            Console.WriteLine($"Delay: {rpcCommand.DelayInMs} TestValue: {rpcCommand.TestValue}");

            await Task.Run(() =>
            {
                Thread.Sleep(rpcCommand.DelayInMs);
            });

            return new ExampleRpcResponse
            {
                ResponseTestValue = rpcCommand.TestValue
            };
        }
    }
}
