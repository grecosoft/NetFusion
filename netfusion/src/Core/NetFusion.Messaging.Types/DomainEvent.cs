using NetFusion.Base.Entity;
using System.Collections.Generic;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Base domain-event that can be published to notify application consumers of an occurrence.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        protected DomainEvent()
        {
            Attributes = new EntityAttributes();
        }

        /// <summary>
        /// Dynamic message attributes that can be associated with the domain-event.
        /// </summary>
        public IEntityAttributes Attributes { get; }

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }
    }
}


