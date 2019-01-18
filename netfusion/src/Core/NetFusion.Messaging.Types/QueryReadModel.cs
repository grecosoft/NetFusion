using NetFusion.Base.Entity;
using System.Collections.Generic;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// An optional model from which a query result can be derived to add  a 
    /// set of dynamic properties.  These dynamic properties can be utilized 
    /// by query filters - for example to add calculated properties based on 
    /// expressions evaluated by Roslyn.
    /// </summary>
    public class QueryReadModel : IAttributedEntity
    {
        /// <summary>
        /// Set of dynamic properties associated with the model.
        /// </summary>
        public IEntityAttributes Attributes { get; }
        
        public QueryReadModel()
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
