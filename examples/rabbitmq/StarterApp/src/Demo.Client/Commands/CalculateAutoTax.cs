using NetFusion.Messaging.Types;

namespace Demo.Client.Commands
{
    public class CalculateAutoTax : Command<TaxCalc>
    {
        public string Vin { get; set; }
        public string ZipCode { get; set; }
    }
}