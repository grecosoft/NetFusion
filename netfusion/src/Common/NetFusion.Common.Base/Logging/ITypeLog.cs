namespace NetFusion.Common.Base.Logging;

/// <summary>
/// Implemented by a type to determine what state should be logged.
/// </summary>
public interface ITypeLog
{
    /// <summary>
    /// Should return subset of a type's state to be logged.
    /// </summary>
    /// <returns>State to be logged.</returns>
    object Log();
}