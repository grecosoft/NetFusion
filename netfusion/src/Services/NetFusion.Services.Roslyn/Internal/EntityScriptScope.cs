using System;
using System.Dynamic;
using NetFusion.Common.Base.Entity;

// ReSharper disable JoinNullCheckWithUsage

namespace NetFusion.Services.Roslyn.Internal;

/// <summary>
/// When evaluating expressions against a given domain model and its set of optional 
/// dynamic properties, the EntityEvaluationService uses this as the execution scope.</summary>
/// <typeparam name="TEntity">The type of the entity that will be evaluated by the script.</typeparam>
public class EntityScriptScope<TEntity>
{
    /// <summary>
    /// The static domain entity type.  The expression specifies Entity.PropName to access
    /// a static property associated with the entity.
    /// </summary>
    public TEntity Entity { get; }

    /// <summary>
    /// The set of dynamic properties.  The expression specifies _.AttributeName to access
    /// a dynamic attribute associated with an entity.
    /// </summary>
    public dynamic _ { get; } 

    public EntityScriptScope(object entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Entity = (TEntity)entity;

        var attributedEntity = entity as IAttributedEntity;
        _ = attributedEntity?.Attributes.Values ?? new ExpandoObject();
    }
}