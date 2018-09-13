using System;

namespace Demo.Domain.Commands
{
    public class TaxCalc
    {
        public decimal Amount { get; set; }
        public DateTime DateCalculated { get; set; }
    }
}
