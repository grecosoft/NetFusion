using Microsoft.Extensions.Logging;

namespace NetFusion.Domain.Patterns.Behaviors.Logging
{
    /// <summary>
    /// Logger used to log messages scoped to the associated domain entity.
    /// This allows an easy way for aggregates and domain entities to write
    /// logs from their internal methods.
    /// </summary>
    public interface ILoggingBehavior
    {
        /// <summary>
        /// Logger used to write aggregate or domain-entity associated logs.
        /// </summary>
        ILogger Logger { get; }
    }
}
