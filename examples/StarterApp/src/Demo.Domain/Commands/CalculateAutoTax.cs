using Demo.Domain.Entities;
using NetFusion.Messaging.Types;

namespace Demo.Domain.Commands
{
    public class CalculateAutoTax : Command<TaxCalc>
    {
        public string Vin { get; set; }
        public string ZipCode { get; set; }
    }
}
