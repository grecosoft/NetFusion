using System;

namespace NetFusion.Domain.Entity
{
    /// <summary>
    /// Represents a dynamic expression script that can be
    /// executed at runtime against a given domain entity
    /// and/or its set of dynamic properties.
    /// </summary>
    public class EntityPropertyExpression
    {
        public EntityPropertyExpression(
            Type entityType,
            string propName,
            string expression,
            string id = null)
        {
            this.EntityType = entityType.AssemblyQualifiedName;
            this.PropertyName = propName;
            this.Expression = expression;
            this.Id = id;
        }

        public EntityPropertyExpression(
            Type entityType,
            string expression) : this(entityType, null, expression)
        {
        }

        // Identity value of the expression.
        public string Id { get; private set; }

        // The CLR type of the domain entity the expression is associated.
        public string EntityType { get; private set; }

        // The name of the defined property.
        public string PropertyName { get; private set; }

        // The expression script containing a short C# expression/calculation
        // that has full access to the static domain entity and its set of 
        // dynamic runtime property values.
        public string Expression { get; private set; }
    }
}
