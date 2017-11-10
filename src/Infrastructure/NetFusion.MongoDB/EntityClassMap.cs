using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using NetFusion.MongoDB.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetFusion.MongoDB
{
    /// <summary>
    /// Derives from the base MongoDB driver mapping class and
    /// adds any additional class mapping properties.
    /// </summary>
    /// <typeparam name="TClass">The type of the entity being mapped.</typeparam>
    public abstract class EntityClassMap<TClass> : BsonClassMap<TClass>,
        IEntityClassMap
    {
        // Additional mapping properties:
        public string CollectionName { get; set; }
        public Type EntityType => typeof(TClass);

        public BsonClassMap ClassMap
        {
            get { return this; }
        }

        /// <summary>
        /// Maps an entity's ID property specified as a string to an ObjectId.
        /// </summary>
        /// <param name="propertyLambda">The ID property to map.</param>
        /// <returns>Reference to the created member map.</returns>
        protected BsonMemberMap MapStringPropertyToObjectId(Expression<Func<TClass, string>> propertyLambda)
        {
            if (propertyLambda == null) throw new ArgumentNullException(nameof(propertyLambda));

            return MapIdProperty(propertyLambda)
                .SetIdGenerator(StringObjectIdGenerator.Instance)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        }

        /// <summary>
        /// Invoked when the mapping is being added to the MongoDB driver.  Allows derived
        /// mapping class to specify any known-types associated with the type of the mapping.
        /// </summary>
        /// <param name="pluginTypes">List of plug-in types to be filtered and added as known-types
        /// by calling the AddKnowType method.
        /// </param>
        public virtual void AddKnownPluginTypes(IEnumerable<Type> pluginTypes)
        {

        }

        /// <summary>
        /// Adds a known derived type of the type being mapped.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        protected void AddKnownType<T>() where T : TClass
        {
            this.AddKnownType(typeof(T));
        }
    }
}
