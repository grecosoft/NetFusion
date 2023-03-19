namespace NetFusion.Common.Base.Entity;

/// <summary>
/// Interface implemented to indicate optimistic concurrency check
/// is required upon update.
/// </summary>
public interface IEntityToken
{
    /// <summary>
    /// The token associated with the current state.
    /// </summary>
    string? Token { get; }
}