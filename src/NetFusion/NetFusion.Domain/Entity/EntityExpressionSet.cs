using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Entity
{
    public class EntityExpressionSet
    {
        public string Id { get; }
        public Type EntityType { get; }
        public string ContextIdentifier { get; }
        public IReadOnlyCollection<EntityExpression> Expressions { get; }

        public EntityExpressionSet(
            string Id,
            string entityType,
            string contextIdentifier,
            IReadOnlyCollection<EntityExpression> expressions) 
        {
            this.Id = Id;
            this.EntityType = Type.GetType(entityType);
            this.ContextIdentifier = contextIdentifier;
            this.Expressions = expressions;
        }
    }
}
