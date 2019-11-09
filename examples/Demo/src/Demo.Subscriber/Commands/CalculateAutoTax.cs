using Demo.Subscriber.Entities;
using NetFusion.Messaging.Types;

namespace Demo.Subscriber.Commands
{
    public class CalculateAutoTax : Command<TaxCalc>
    {
        public string Vin { get; set; }
        public string ZipCode { get; set; }
    }
}
