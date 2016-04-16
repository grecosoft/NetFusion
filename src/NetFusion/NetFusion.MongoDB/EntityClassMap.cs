using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using NetFusion.Common;
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
        public Type EntityType { get { return typeof(TClass); } }

        public BsonClassMap ClassMap
        {
            get { return this; }
        }

        public BsonMemberMap MapStringObjectIdProperty(Expression<Func<TClass, string>> propertyLambda)
        {
            Check.NotNull(propertyLambda, nameof(propertyLambda), "property selector not specified");

            return this.MapIdProperty(propertyLambda)
                .SetIdGenerator(StringObjectIdGenerator.Instance)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        }

        public virtual void AddKnownPluginTypes(IEnumerable<Type> pluginTypes)
        {

        }

        protected void AddDrivedType<T>() where T : TClass
        {
            this.AddKnownType(typeof(T));
        }

    }
}
