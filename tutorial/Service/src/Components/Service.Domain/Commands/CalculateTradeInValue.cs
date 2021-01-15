using NetFusion.Messaging.Types;

namespace Service.Domain.Commands
{
    [MessageNamespace("Calculations.Auto.TradeIn")]
    public class CalculateTradeInValue : Command<TradeInResult>    
    {
        public string Make { get; set; }
        public int Year { get; set; }
    }
}