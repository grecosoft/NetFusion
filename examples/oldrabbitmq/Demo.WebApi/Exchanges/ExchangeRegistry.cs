using Demo.WebApi.Commands;
using Demo.WebApi.DomainEvents;
using NetFusion.Base;
using NetFusion.RabbitMQ.Publisher;

namespace Demo.WebApi.Exchanges
{
    public class ExchangeRegistry : ExchangeRegistryBase
    {
        protected override void OnRegister()
        {
            DefineTopicExchange<AutoSaleCompleted>("CompletedAutoSales", "testBus");
            
//            
//            DefineTopicExchange<TestDomainEvent>("AutoSales", "testBus", 
//                c => c.ContentType = SerializerTypes.MessagePack);
            
//            DefineWorkQueue<TestCommand>("GeneratePdf", "testBus");
//            DefineFanoutExchange<NotificationDomainEvent>("PartRequested", "testBus");
//            DefineRpcExchange<GetPropertyTaxCommand>("RpcRequests", "CalculatePropertyTax", "testBus");
//            DefineRpcExchange<GetSalesTaxCommand>("RpcRequests", "CalculateSalesTax", "testBus");
        }
    }
}