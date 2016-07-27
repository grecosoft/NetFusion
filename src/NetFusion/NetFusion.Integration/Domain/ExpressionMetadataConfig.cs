using NetFusion.Domain.Entity;
using System;

namespace NetFusion.Integration.Domain
{
    /// <summary>
    /// Data model used to store a set of related domain entity expression.
    /// </summary>
    public class ExpressionMetadataConfig
    {
        public string Id { get; private set; }
        public string EntityType { get; private set; }
        public string PropertyName { get; private set; }
        public string Expression { get; private set; }

        public ExpressionMetadataConfig(
            string entityType,
            string propertyName,
            string expression)
        {
            this.EntityType = entityType;
            this.PropertyName = propertyName;
            this.Expression = expression;
        }

        public EntityPropertyExpression ToEntity()
        {
            return new EntityPropertyExpression(
                Type.GetType(this.EntityType), 
                this.PropertyName, 
                this.Expression, 
                this.Id);
        }
    }
}

// Add context, sequence, and IsPersistant 
// Assemblies and imports to reference.
// Repository to read roles from JSON file
// See how to have mongoDB driver user camelCase
// Determine best document structure?  Document per entity type?
