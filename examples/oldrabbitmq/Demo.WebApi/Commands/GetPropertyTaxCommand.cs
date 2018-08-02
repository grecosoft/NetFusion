using NetFusion.Messaging.Types;

namespace Demo.WebApi.Commands
{
    public class GetPropertyTaxCommand : Command<TaxCalculations>
    {
        public int InputValue { get; set; }
    
       public GetPropertyTaxCommand()
       {
           InputValue = 1000;
       }
    }
    
    public class GetSalesTaxCommand : Command<TaxCalculations>
    {
        public int InputValue { get; set; }
    
        public GetSalesTaxCommand()
        {
            InputValue = 1000;
        }
    }
}