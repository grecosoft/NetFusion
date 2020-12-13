using NetFusion.RabbitMQ.Publisher;
using Service.Domain.Commands;
using Service.Domain.Events;

namespace Service.Infra
{
    public class ExchangeRegistry : ExchangeRegistryBase
    {
        protected override void OnRegister()
        {
            DefineDirectExchange<PropertySold>("RealEstate", "testBus", settings =>
            {
                settings.IsNonRoutedSaved = true;
            });
            
            DefineTopicExchange<AutoSaleCompleted>("CompletedAutoSales", "testBus");
            DefineFanoutExchange<TemperatureReading>("TemperatureReading", "testBus");
            DefineWorkQueue<SendEmail>("GeneratedAndSendEmail", "testBus");
            
            DefineRpcQueue<CalculatePropertyTax>("TaxCalculations", "Business.Calcs.Taxes.Property", "testBus");
            DefineRpcQueue<CalculateAutoTax>("TaxCalculations", "Business.Calcs.Taxes.Auto", "testBus");
        }
    }
}