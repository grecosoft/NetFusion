using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetFusion.Common.Exceptions
{
    [Serializable]
    public class NetFusionException : Exception
    {
        public IDictionary<string, object> Details { get; protected set; }

        public NetFusionException()
        {

        }

        public NetFusionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
           
        }

        public NetFusionException(string message)
            : base(message)
        {

        }

        public NetFusionException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public NetFusionException(string message, object details)
            : base(message)
        {
            this.Details = details.ToDictionary();
        }

        public NetFusionException(string message, object details, Exception innerException)
            : base(message, innerException)
        {
            this.Details = details.ToDictionary();
        }
    }
}
