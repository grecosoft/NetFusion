using NetFusion.Base.Entity;
using System.Collections.Generic;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Base domain-event that can be published to notify application component consumers of an occurrence.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        private IEntityAttributes _attributes;

        public DomainEvent()
        {
            _attributes = new EntityAttributes();
        }

        /// <summary>
        /// Dynamic message attributes that can be associated with the domain-event.
        /// </summary>
        public IEntityAttributes Attributes => _attributes;

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get { return _attributes.GetValues(); }
            set { _attributes.SetValues(value); }
        }
    }
}
