using NetFusion.Messaging;

namespace RefArch.Api.RabitMQ.Messages
{
    public class ExampleRpcResponse: DomainEvent
    {
        public string ResponseTestValue { get; set; }
    }
}
