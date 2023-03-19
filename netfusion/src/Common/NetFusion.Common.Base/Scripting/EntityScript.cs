using System;
using System.Collections.Generic;

namespace NetFusion.Common.Base.Scripting;

/// <summary>
/// Represents a set of ordered expressions executed at runtime against a domain entity.
/// </summary>
public class EntityScript
{
    /// <summary>
    /// Identity value for the script.
    /// </summary>
    public string ScriptId { get; }

    /// <summary>
    /// The name identifying the script.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The fully qualified type of the entity to which the script is associated.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// The expressions associated with the script.
    /// </summary>
    public IReadOnlyCollection<EntityExpression> Expressions { get; }
        
    /// <summary>
    /// Creates new script.
    /// </summary>
    /// <param name="scriptId">Identity value for the script.</param>
    /// <param name="name">The name identifying the script.</param>
    /// <param name="entityTypeName">The type of the entity to which the script is associated.</param>
    /// <param name="expressions">The expressions associated with the script.</param>
    public EntityScript(
        string scriptId,
        string name,
        string entityTypeName,
        IReadOnlyCollection<EntityExpression> expressions) 
    {
        ScriptId = scriptId ?? throw new ArgumentNullException(nameof(scriptId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Expressions = expressions ?? throw new ArgumentNullException(nameof(expressions));
            
        EntityType = Type.GetType(entityTypeName, false) ?? throw new InvalidOperationException(
            $"The type of: {entityTypeName} associated with script named {name} could not be loaded.");
    }

    /// <summary>
    /// Description of the script within the current application domain.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The dynamic entity attributes and their initial values that should be used
    /// if not set before the script is executed.
    /// </summary>
    public IDictionary<string, object> InitialAttributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// The assembles that can be accessed by the script.
    /// </summary>
    public ICollection<string>? ImportedAssemblies { get; set; }

    /// <summary>
    /// The namespaces that can be referenced by the script.
    /// </summary>
    public ICollection<string>? ImportedNamespaces { get; set; }
}