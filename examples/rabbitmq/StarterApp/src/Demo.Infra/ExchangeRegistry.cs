using Demo.App.Commands;
using Demo.App.DomainEvents;
using NetFusion.RabbitMQ.Publisher;

namespace Demo.Infra
{
    public class ExchangeRegistry : ExchangeRegistryBase
    {
        protected override void OnRegister()
        {
            DefineTopicExchange<AutoSaleCompleted>("CompletedAutoSales", "testBus");
            DefineDirectExchange<PropertySold>("RealEstate", "testBus");
            DefineRpcQueue<CalculatePropertyTax>("TaxCalculations", "PropertyTax", "testBus");
            DefineFanoutExchange<TemperatureReading>("TemperatureReading", "testBus");
            DefineWorkQueue<SendEmail>("GeneratedAndSendEmail", "testBus");
        }
    }
}