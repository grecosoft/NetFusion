using Demo.Domain.Entities;
using NetFusion.Messaging.Types;

namespace Demo.Domain.Commands
{
    public class CalculatePropertyTax : Command<TaxCalc>
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}
