namespace Service.Domain.Commands
{
    using System;

    public class TaxCalc
    {
        public decimal Amount { get; set; }
        public DateTime DateCalculated { get; set; }
    }
}