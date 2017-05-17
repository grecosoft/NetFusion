namespace NetFusion.Domain.Entities.Registration
{
    /// <summary>
    /// Responsible for injected domain services into domain-entity behaviors.
    /// This should not be used to inject application services.  
    /// </summary>
    public interface IDomainServiceResolver
    {
        /// <summary>
        /// Implementation should resolve any domain services defined for a behavior.
        /// </summary>
        /// <param name="domainBehavior">Reference to the domain-event behavior.</param>
        void ResolveDomainServices(IDomainBehavior domainBehavior);
    }
}
