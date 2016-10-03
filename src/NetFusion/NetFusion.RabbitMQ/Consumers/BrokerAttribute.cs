using NetFusion.Common;
using System;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Attached to message consumer class to specify the broker to which the 
    /// message handlers are associated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class BrokerAttribute : Attribute
    {
        /// <summary>
        /// The name of the broker specified in BrokerSettings.
        /// </summary>
        public string BrokerName { get; }

        /// <summary>
        /// The name of the broker specified in BrokerSettings.
        /// </summary>
        /// <param name="brokerName">The name of the broker.</param>
        public BrokerAttribute(string brokerName)
        {
            Check.NotNullOrWhiteSpace(brokerName, nameof(brokerName));

            this.BrokerName = brokerName;
        }
    }
}
