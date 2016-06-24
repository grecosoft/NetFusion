using NetFusion.Messaging;

namespace RefArch.Api.Messages
{
    public class ExampleRpcResponse: DomainEvent
    {
        public string Comment { get; set; }
    }
}
