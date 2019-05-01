using NetFusion.Base.Entity;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Base domain-event that can be published to notify application consumers of an occurrence.
    /// Domain-events are used to notify one or more consumers of an occurrence of an event.
    /// Often, command handler will publish domain-events used to notify interested subscribers
    /// of state changes made by the handling of the command.
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
        [IgnoreDataMember]
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


