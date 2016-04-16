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
        private IDictionary<string, object> _attributes;

        public IDictionary<string, object> Attributes
        {
            get
            {
                return _attributes ?? (_attributes = new Dictionary<string, object>());
            }
        }
    }
}
