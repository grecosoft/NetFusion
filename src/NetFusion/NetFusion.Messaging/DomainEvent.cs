using NetFusion.Domain.Entity;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Base domain-event that can be published.
    /// </summary>
    [Serializable]
    public abstract class DomainEvent : IDomainEvent
    {
        private IEntityAttributes _attributes;

        public DomainEvent()
        {
            _attributes = new EntityAttributes();
        }

        public IEntityAttributes Attributes => _attributes;

        public IDictionary<string, object> AttributeValues
        {
            get { return _attributes.GetValues(); }
            set { _attributes.SetValues(value); }
        }
    }
}
