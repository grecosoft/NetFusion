using NetFusion.Common.Extensions;
using NetFusion.Messaging;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Consumers;
using RefArch.Api.Messages.RabbitMQ;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RefArch.Subscriber.Services
{
    [Broker("TestBroker")]
    public class ExampleRpcService : IMessageConsumer
    {
        [JoinQueue("QueueWithConsumerResponse", "SampleRpcExchange")]
        public async Task<ExampleRpcResponse> OnRpcMessage(ExampleRpcCommand rpcCommand)
        {
            Console.WriteLine($"Handler: OnRpcMessage: { rpcCommand.ToIndentedJson()}");

            rpcCommand.SetAcknowledged();

            await Task.Run(() =>
            {
                Thread.Sleep(500);
            });

            return new ExampleRpcResponse
            {
                Comment = "World"
            };
        }
    }
}
