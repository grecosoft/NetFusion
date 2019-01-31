using System;

namespace NetFusion.AMQP.Subscriber.Internal
{
    /// <summary>
    /// Base class from which derived attributes used to specify message
    /// handler methods associated with a host item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public abstract class HostItemAttribute : Attribute
    {
        /// <summary>
        /// The credit value used when linking to a host item.
        /// </summary>
        public int LinkCredit { get; set; } = 10;
        
        /// <summary>
        /// Reference to an object that knows how to link the handler
        /// method to the host item.
        /// </summary>
        public ISubscriberLinker Linker { get; }

        protected HostItemAttribute(ISubscriberLinker linker)
        {
            Linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }
    }
}