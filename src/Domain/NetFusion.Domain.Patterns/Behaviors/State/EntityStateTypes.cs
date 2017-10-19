namespace NetFusion.Domain.Patterns.Behaviors.State
{
    /// <summary>
    /// The state associated with an entity contained within an aggregate.
    /// </summary>
    public enum EntityStateTypes
    {
        Added = 1,
	    Updated = 2,
	    Removed = 3
    }
}
