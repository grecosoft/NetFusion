using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;

namespace NetFusion.Common.Exceptions
{
    public class NetFusionException : Exception
    {
        public IEnumerable<object> Details { get; private set; }

        public NetFusionException(string message)
            : base(FormatMessage(message))
        {

        }

        public NetFusionException(string message, Exception innerException)
            : base(FormatMessage(message), innerException)
        {

        }

        public NetFusionException(string message, object details)
            : base(FormatMessage(message, details))
        {
            
        }

        public NetFusionException(string message, object details, Exception innerException)
            : base(FormatMessage(message, details), innerException)
        {

        }

        private static string FormatMessage(string message, object details = null)
        {
            return new
            {
                Message = message,
                Details = details
            
            }.ToIndentedJson();
        }
    }
}
