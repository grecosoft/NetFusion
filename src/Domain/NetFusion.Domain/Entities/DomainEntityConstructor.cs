using NetFusion.Common;
using NetFusion.Domain.Entities.Core;
using System;

namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Returned by the domain entity factory for a specific domain-entity type to be created.  
    /// The returned instance has factory methods used for constructing and instance of the 
    /// domain-entity.
    /// </summary>
    /// <typeparam name="TDomainEntity"></typeparam>
    public class DomainEntityConstructor<TDomainEntity>
        where TDomainEntity : IBehaviorDelegator
    {
        private Action<TDomainEntity> _prepareEntity;

        public DomainEntityConstructor(Action<TDomainEntity> prepareEntity)
        {
            _prepareEntity = prepareEntity;
        }

        /// <summary>
        /// Creates an instance of a domain-entity by delegating to a domain-entity
        /// builder class.  The builder class is often a child class of the domain
        /// entity being created.  This allows the builder class to have access to
        /// the private state of the domain-entity being created.
        /// </summary>
        /// <typeparam name="TBuilder">The builder class responsible for creating
        /// and instance of the domain entity.</typeparam>
        /// <param name="buildStrategy">Function passing an instance of the builder class
        /// to the calling code.  The calling code invokes one or more builder methods
        /// to build an instance of the domain entity that it returns.</param>
        /// <returns>Instance of the created domain-entity.</returns>
        public TDomainEntity Using<TBuilder>(Func<TBuilder, TDomainEntity> buildStrategy)
            where TBuilder : IDomainEntityBuilder<TDomainEntity>, new()
        {
            Check.NotNull(buildStrategy, nameof(buildStrategy), "Caller builder strategy method not specified.");

            var entityBuilder = new TBuilder();
            TDomainEntity domainEntity = buildStrategy(entityBuilder);

            _prepareEntity(domainEntity);
            return domainEntity;
        }
    }
}
