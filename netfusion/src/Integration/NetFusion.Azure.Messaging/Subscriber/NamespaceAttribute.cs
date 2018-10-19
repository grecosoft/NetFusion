using System;

namespace NetFusion.Azure.Messaging.Subscriber
{
    /// <summary>
    /// Attribute specified on a class implementing the IMessageConsumer marker
    /// interface used to specify the Azure Service Bus configuration configured
    /// in the application's configuration settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NamespaceAttribute : Attribute
    {
        /// <summary>
        /// The name of the namespace specified within the application's settings.
        /// </summary>
        public string NamespaceName { get; }

        public NamespaceAttribute(string namespaceName)
        {
            NamespaceName = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
        }
    }
}