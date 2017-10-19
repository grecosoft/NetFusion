using NetFusion.Domain.Entities;

namespace NetFusion.Domain.Patterns.Behaviors.Mapping
{
    /// <summary>
    /// Behavior that can be associated with a domain entity allowing it to be mapped to a target type.
    /// </summary>
    public interface IMappingBehavior : IDomainBehavior
    {
        /// <summary>
        /// Maps the domain entity to the specified target type.
        /// </summary>
        /// <typeparam name="TTarget">The target type to map entity.</typeparam>
        /// <returns>Instance of the target type.</returns>
        TTarget MapTo<TTarget>() where TTarget : class, new();
    }
}
