using System;

namespace NetFusion.Azure.Messaging.Subscriber.Internal
{
    /// <summary>
    /// Base class from which derived attributes used to specify message
    /// handler methods associated with a namespace item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class NamespaceItemAttribute : Attribute
    {
        /// <summary>
        /// The credit value used when linking to a namespace item.
        /// </summary>
        public int LinkCredit { get; set; } = 10;
        
        /// <summary>
        /// Reference to an object that knows how to like the handler
        /// method to the namespace item.
        /// </summary>
        public ISubscriberLinker Linker { get; }

        protected NamespaceItemAttribute(ISubscriberLinker linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }
    }
}