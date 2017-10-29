using NetFusion.Domain.Entities.Core;

namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Implemented by a class, delegated to by the domain-entity factory, 
    /// responsible for creating an instance of a domain entity.  Often
    /// the class is a child class of the domain-entity it creates.  This
    /// allows the builder class to have access to the private state of 
    /// the domain-entity being created.
    /// </summary>
    /// <typeparam name="TDomainEntity">The type of domain entity being created.</typeparam>
    public interface IDomainEntityBuilder<TDomainEntity>
        where TDomainEntity : IBehaviorDelegator
    {

    }
}
