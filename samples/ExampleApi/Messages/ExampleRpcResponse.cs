using NetFusion.Messaging.Types;

namespace ExampleApi.Messages
{
    public class ExampleRpcResponse: DomainEvent
    {
        public string ResponseTestValue { get; set; }
    }
}
