using NetFusion.Common;
using NetFusion.Domain.Entities.Registration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Factory used to create domain entity instances.  The created domain-entity will have 
    /// any behaviors registered for the entity type associated with the created instance.
    /// </summary>
    public class DomainEntityFactory : IDomainEntityFactory,
        IFactoryRegistry
    {
        /// <summary>
        /// Reference to the factory instance creating during the bootstrap process.
        /// </summary>
        public static IDomainEntityFactory Instance { get; private set; }

        private IDomainServiceResolver _resolver;
        private Dictionary<Type, SupportedBehavior> _entityBehaviors = new Dictionary<Type, SupportedBehavior>();
        private Dictionary<Type, SupportedBehavior> _factoryBehaviors = new Dictionary<Type, SupportedBehavior>();

        public DomainEntityFactory(IDomainServiceResolver resolver)
        {
            Check.NotNull(resolver, nameof(resolver));
            _resolver = resolver;
        }

        /// <summary>
        /// Called during the bootstrap process to set the singleton factory instance.
        /// </summary>
        /// <param name="factory"></param>
        public static void SetInstance(IDomainEntityFactory factory)
        {
            Check.NotNull(factory, nameof(factory));
            Instance = factory;
        }

        public TDomainEntity Create<TDomainEntity>()
            where TDomainEntity : IEntityDelegator, new()
        {
            var supportedBehaviors = new Lazy<IEnumerable<EntityBehavior>>( () => GetBehaviors(typeof(TDomainEntity)));

            TDomainEntity domainEntity = new TDomainEntity();
            IEntity entity = new Entity(domainEntity, _resolver, supportedBehaviors);

            domainEntity.SetEntity(entity);
            return domainEntity;
        }

        private IEnumerable<EntityBehavior> GetBehaviors(Type domainEntityType)
        {
            SupportedBehavior domainEntity = null;

            _entityBehaviors.TryGetValue(domainEntityType, out domainEntity);

            var entityBehaviors = domainEntity != null ? domainEntity.SupportedBehaviors.ToList()
                : new List<Behavior>();

            // Add any general factory behaviors for which there is not already
            // an entity specific behavior.
            foreach (var factoryBehavior in _factoryBehaviors)
            {
                if (!entityBehaviors.Any(b => b.ContractType == factoryBehavior.Key))
                {
                    entityBehaviors.AddRange(factoryBehavior.Value.SupportedBehaviors);
                }
            }

            return entityBehaviors.Select(b => new EntityBehavior(b));
        }

        public void BehaviorsFor<TDomainEntity>(Action<ISupportedBehavior> entity) where TDomainEntity : IEntityDelegator
        {
            Check.NotNull(entity, nameof(entity));

            var domainEntityType = typeof(TDomainEntity);
            SupportedBehavior entityReg = null;

            if (!_entityBehaviors.TryGetValue(domainEntityType, out entityReg))
            {
                entityReg = new SupportedBehavior(domainEntityType);
                _entityBehaviors[domainEntityType] = entityReg;
            }

            entity(entityReg);
        }

        public IFactoryRegistry AddBehavior<TContract, TBehavior>() where TBehavior : TContract
        {
            var supportedBehavior = new SupportedBehavior();
            supportedBehavior.Supports<TContract, TBehavior>();

            _factoryBehaviors[typeof(TContract)] = supportedBehavior;

            return this;
        }
    }
}
