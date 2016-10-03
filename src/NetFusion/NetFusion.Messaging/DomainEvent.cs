using NetFusion.Domain.Entity;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Base domain-event that can be published to notify application
    /// component consumers of an occurrence.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent : IDomainEvent
    {
        private IEntityAttributes _attributes;

        public DomainEvent()
        {
            _attributes = new EntityAttributes();
        }

        /// <summary>
        /// Dynamic message attributes.
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
