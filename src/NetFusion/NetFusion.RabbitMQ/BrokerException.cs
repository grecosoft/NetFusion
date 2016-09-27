using NetFusion.Common.Exceptions;
using System;

namespace NetFusion.RabbitMQ
{
    /// <summary>
    /// Exception that is thrown when there is an issue configuring the
    /// message broker or publishing and receiving messages from the
    /// message broker.
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
