namespace NetFusion.Common.Base.Scripting;

/// <summary>
/// Properties describing a script to be executed and the value of an entity
/// attribute containing the result of the Boolean expression.
/// </summary>
/// <param name="ScriptName">The name of the script that should be executed against a source object
/// to determine if it meets the criteria of the expression.</param>
/// <param name="AttributeName">The entity's Boolean attribute that determines if the source object
/// matches the criteria of the script.</param>
public record ScriptPredicate(string ScriptName, string AttributeName);