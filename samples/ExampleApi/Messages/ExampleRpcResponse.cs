using NetFusion.Domain.Messaging;

namespace ExampleApi.Messages
{
    public class ExampleRpcResponse: DomainEvent
    {
        public string ResponseTestValue { get; set; }
    }
}
