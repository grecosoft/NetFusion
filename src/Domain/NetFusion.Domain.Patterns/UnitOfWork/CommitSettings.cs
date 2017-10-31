namespace NetFusion.Domain.Patterns.UnitOfWork
{
    /// <summary>
    /// Settings that can be passed when committing an unit-of-work.
    /// </summary>
    public class CommitSettings
    {
        /// <summary>
        /// Determines if an exception should be throw if any of the enlisted aggregates have 
        /// error validations.  If false, the validations for all enlisted aggregates will be 
        /// returned as part of the commit result.  Commitment stops at the first aggregate 
        /// containing an error exception.  Non-error level types do not stop the unit-of-work
        /// from being committed.
        /// </summary>
        public bool ThrowIfInvalid { get; set; } = true;
    }
}
