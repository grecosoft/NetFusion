using NetFusion.Common.Exceptions;
using System;

namespace NetFusion.Bootstrap.Exceptions
{
    /// <summary>
    /// Exception thrown by the container when there is an issue bootstrapping
    /// the application.
    /// </summary>
    public class ContainerException : NetFusionException
    {
        public ContainerException(string message)
            : base(message)
        {

        }

        public ContainerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public ContainerException(string message, object details)
            : base(message, details)
        {

        }

        public ContainerException(string message, object details, Exception innerException)
            : base(message, details, innerException)
        {

        }
    }
}
