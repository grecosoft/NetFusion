using System;
using Demo.Subscriber.Commands;
using Demo.Subscriber.Entities;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Subscriber
{
    public class SampleRpcHandler : IMessageConsumer
    {
        [RpcQueue("testBus", "TaxCalculations", "Business.Calcs.Taxes.Property")]
        public TaxCalc CalculatePropertyTax(CalculatePropertyTax command)
        {
            Console.WriteLine(command.State);

            return command.State == "CT" ? new TaxCalc { Amount = 20_500 }
                : new TaxCalc { Amount = 3_000 };
        }

        [RpcQueue("testBus", "TaxCalculations", "Business.Calcs.Taxes.Auto")]
        public TaxCalc CalculateAutoTax(CalculateAutoTax command)
        {
            Console.WriteLine(command.ZipCode);

            return command.ZipCode == "06410" ? new TaxCalc { Amount = 1_000 }
                : new TaxCalc { Amount = 200 };
        }
    }
}
