using System;

#if NET461
using System.Runtime.Serialization;
#endif

namespace NetFusion.Base.Exceptions
{
#if NET461
    [Serializable]
#endif
    public class OptimisticConcurrencyException : NetFusionException
    {
        internal OptimisticConcurrencyException(string message, params object[] args)
            : base(String.Format(message, args))
        {

        }
    }
}
