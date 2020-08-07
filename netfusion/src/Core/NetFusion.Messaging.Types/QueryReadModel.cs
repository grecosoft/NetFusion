using NetFusion.Base.Entity;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// An optional model from which a query result can be derived to add a 
    /// set of dynamic properties. 
    /// </summary>
    public abstract class QueryReadModel : IAttributedEntity
    {
        /// <summary>
        /// Set of dynamic properties associated with the model.
        /// </summary>
        [JsonIgnore]
        public IEntityAttributes Attributes { get; }

        protected QueryReadModel()
        {
            Attributes = new EntityAttributes();
        }

        /// <summary>
        /// Used to set and retrieve the properties associated with the module.
        /// Mostly used during serialization.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }
    }
}
