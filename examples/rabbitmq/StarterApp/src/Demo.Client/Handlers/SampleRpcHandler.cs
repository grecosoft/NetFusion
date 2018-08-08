using Demo.Client.Commands;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Subscriber;

namespace Demo.Client.Handlers
{
    public class SampleRpcHandler : IMessageConsumer
    {

        [RpcQueue("testBus", "TaxCalculations", "PropertyTax")]
        public TaxCalc CalculatePropertyTax(CalculatePropertyTax command)
        {
            return new TaxCalc
            {    
                Amount = 5000
            };
        }
    }
}