using System;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;
using Service.Client.Commands;

namespace Service.Client.Handlers
{
    public class SampleRpcHandler : IMessageConsumer
    {
        [RpcQueue("testBus", "TaxCalculations", "Business.Calcs.Taxes.Property")]
        public TaxCalc CalculatePropertyTax(CalculatePropertyTax command)
        {
            Console.WriteLine(command.State);

            return command.State == "CT" ? new TaxCalc { Amount = 20_500, DateCalculated = DateTime.Now }
                : new TaxCalc { Amount = 3_000, DateCalculated = DateTime.Now };
        }

        [RpcQueue("testBus", "TaxCalculations", "Business.Calcs.Taxes.Auto")]
        public TaxCalc CalculateAutoTax(CalculateAutoTax command)
        {
            Console.WriteLine(command.ZipCode);

            return command.ZipCode == "06410" ? new TaxCalc { Amount = 1_000, DateCalculated = DateTime.Now }
                : new TaxCalc { Amount = 200, DateCalculated = DateTime.Now };
        }
    }
}