using System;

namespace NetFusion.AMQP.Subscriber
{
    /// <summary>
    /// Attribute specified on a class implementing the IMessageConsumer marker
    /// interface used to specify the Amqp host configuration defined in the
    /// application's configuration settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HostAttribute : Attribute
    {
        /// <summary>
        /// The name of the host specified within the application's settings.
        /// </summary>
        public string HostName { get; }

        public HostAttribute(string hostName)
        {
            HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
        }
    }
}