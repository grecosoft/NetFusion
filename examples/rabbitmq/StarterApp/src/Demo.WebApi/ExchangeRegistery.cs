using Demo.Domain.Events;
using Demo.Domain.Commands;
using NetFusion.RabbitMQ.Publisher;

namespace Demo.WebApi
{
    public class ExchangeRegistry : ExchangeRegistryBase
    {
        protected override void OnRegister()
        {
            DefineDirectExchange<PropertySold>("RealEstate", "testBus");
	    DefineTopicExchange<AutoSaleCompleted>("CompletedAutoSales", "testBus");
            DefineFanoutExchange<TemperatureReading>("TemperatureReading", "testBus");
            DefineWorkQueue<SendEmail>("GeneratedAndSendEmail", "testBus");
            DefineRpcQueue<CalculatePropertyTax>("TaxCalculations", "Business.Calcs.Taxes.Property", "testBus");
            DefineRpcQueue<CalculateAutoTax>("TaxCalculations", "Business.Calcs.Taxes.Auto", "testBus");
        }
    }
}
