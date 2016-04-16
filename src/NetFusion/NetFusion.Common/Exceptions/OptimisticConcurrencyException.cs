using System;

namespace NetFusion.Common.Exceptions
{
    public class OptimisticConcurrencyException : NetFusionException
    {
        internal OptimisticConcurrencyException(string message, params object[] args)
            : base(String.Format(message, args))
        {

        }
    }
}
