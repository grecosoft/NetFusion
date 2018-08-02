using NetFusion.Messaging.Types;

namespace Demo.Client
{
    public class GetPropertyTaxCommand : Command<TaxCalculations>
    {
       public int InputValue { get; set; }
    }
}