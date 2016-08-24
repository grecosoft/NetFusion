using NetFusion.Common.Exceptions;
using System;

namespace NetFusion.RabbitMQ
{
    /// <summary>
    /// Exception that is thrown when there is an issue configuring or 
    /// processing a published message.
    /// </summary>
    public class BrokerException : NetFusionException
    {
        public BrokerException(string message)
            : base(message)
        {

        }

        public BrokerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public BrokerException(string message, object details)
            : base(message, details)
        {

        }

        public BrokerException(string message, object details, Exception innerException)
            : base(message, details, innerException)
        {

        }
    }
}
