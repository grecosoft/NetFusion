namespace Service.Client.Commands
{
    using NetFusion.Messaging.Types;

    public class CalculateAutoTax : Command<TaxCalc>
    {
        public string Vin { get; set; }
        public string ZipCode { get; set; }
    }
}