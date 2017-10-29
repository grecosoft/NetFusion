namespace NetFusion.Domain.Patterns.UnitOfWork
{
    /// <summary>
    /// Settings that can be passed when committing an unit-of-work.
    /// </summary>
    public class CommitSettings
    {
        /// <summary>
        /// Determines if an exception should be throw if any of the enlisted
        /// aggregates have error validations.
        /// </summary>
        public bool ThrowIfInvalid { get; set; } = true;
    }
}
