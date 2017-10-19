using NetFusion.Domain.Entities.Core;

namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Represents the concept of an aggregate in DDD that encapsulates a set
    /// of related entities for which it manages.  An aggregate can optionally
    /// have a set of related behaviors that can be queried at runtime.  The
    /// NetFusion.Patterns assembly and namespace contains implementation of
    /// some patterns that can be registered with the aggregate.
    /// </summary>
    public interface IAggregate: IBehaviorDelegator
    {
    }

    /// <summary>
    /// Represents an aggregate with an identity value.  For an aggregate,
    /// it is best that the identity is globally unique.
    /// </summary>
    /// <typeparam name="TId">The type of the identity value.</typeparam>
    public interface IAggregate<TId> : IAggregate
    {
        TId Id { get; }
    }
}
