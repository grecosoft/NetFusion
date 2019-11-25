using Demo.Domain.Events;
using Demo.Domain.Commands;
using NetFusion.RabbitMQ.Publisher;

namespace Demo.Infra
{
    public class ExchangeRegistry : ExchangeRegistryBase
    {
        protected override void OnRegister()
        {
            DefineDirectExchange<PropertySold>("RealEstate", "testBus");
            DefineTopicExchange<AutoSaleCompleted>("CompletedAutoSales", "testBus");
            DefineWorkQueue<SendEmail>("GenerateAndSendEmail", "testBus");
            DefineFanoutExchange<TemperatureReading>("TemperatureReading", "testBus");
            DefineRpcQueue<CalculatePropertyTax>("TaxCalculations", "Business.Calcs.Taxes.Property", "testBus");
            DefineRpcQueue<CalculateAutoTax>("TaxCalculations", "Business.Calcs.Taxes.Auto", "testBus");
        }
    }
}
