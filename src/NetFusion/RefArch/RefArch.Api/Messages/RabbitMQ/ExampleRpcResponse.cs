﻿using NetFusion.Messaging;

namespace RefArch.Api.Messages.RabbitMQ
{
    public class ExampleRpcResponse: DomainEvent
    {
        public string Comment { get; set; }
    }
}