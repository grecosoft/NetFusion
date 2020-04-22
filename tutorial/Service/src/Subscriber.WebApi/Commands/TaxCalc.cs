using System;

namespace Subscriber.WebApi.Commands
{
    public class TaxCalc
    {
        public decimal Amount { get; set; }
        public DateTime DateCalculated { get; set; }
    }
}