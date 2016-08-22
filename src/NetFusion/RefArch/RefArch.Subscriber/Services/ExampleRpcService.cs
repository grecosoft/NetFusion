﻿using NetFusion.Common.Extensions;
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
