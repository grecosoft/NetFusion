using NetFusion.Domain.Entities.Registration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Factory used to create domain aggregates or entity instances.  The created entity will 
    /// have any behaviors registered for the entity type associated with the created instance.
    /// </summary>
    public class DomainEntityFactory : IDomainEntityFactory,
        IFactoryRegistry
    {
        /// <summary>
        /// Reference to the factory instance created during the bootstrap process.
        /// </summary>
        public static IDomainEntityFactory Instance { get; private set; }

        // Used to inject services into behavior instances.  Any behavior instance having public properties
        // of types registered in the container will be injected.  
        private IDomainServiceResolver _resolver;

        // Behaviors registered at the factory and entity levels.
        private readonly Dictionary<Type, SupportedBehaviors> _factoryBehaviors;  // BehaviorType => SupportedBehavior (1..1)
        private readonly Dictionary<Type, SupportedBehaviors> _entityBehaviors;   // EntityType => SupportedBehavior (1..*)

        public DomainEntityFactory(IDomainServiceResolver resolver)
        {            
            _factoryBehaviors = new Dictionary<Type, SupportedBehaviors>();
            _entityBehaviors = new Dictionary<Type, SupportedBehaviors>();

            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// Called during the bootstrap process to set the singleton factory instance.
        /// </summary>
        /// <param name="factory">The created singleton domain-entity factory instance.</param>
        public static void SetInstance(IDomainEntityFactory factory)
        {
            Instance = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public TDomainEntity Create<TDomainEntity>()
            where TDomainEntity : IBehaviorDelegator, new()
        {
            TDomainEntity domainEntity = new TDomainEntity();
            SetBehaviorDelegatee(domainEntity);

            return domainEntity;
        }

        public DomainEntityConstructor<TDomainEntity> Build<TDomainEntity>() where TDomainEntity : IBehaviorDelegator
        {
            return new DomainEntityConstructor<TDomainEntity>(domainEntity => {
                SetBehaviorDelegatee(domainEntity);
            });
        }

        public void Build(IBehaviorDelegator domainEntity)
        {
            if (domainEntity.Behaviors != null)
            {
                return;
            }

            SetBehaviorDelegatee(domainEntity);
        }

        private void SetBehaviorDelegatee(IBehaviorDelegator domainEntity)
        {
            // When an entity's behaviors are first accessed, the supported behavior types are loaded.
            // The behavior instances are not created until first used.
            var supportedBehaviors = new Lazy<IEnumerable<EntityBehavior>>(() => GetSupportedBehaviors(domainEntity.GetType()));

            IBehaviorDelegatee delegatee = new BehaviorDelegatee(domainEntity, supportedBehaviors, _resolver);
            domainEntity.SetDelegatee(delegatee);
        }

        private IEnumerable<EntityBehavior> GetSupportedBehaviors(Type domainEntityType)
        {
            _entityBehaviors.TryGetValue(domainEntityType, out SupportedBehaviors entityBehaviors);

            var supportedBehaviors = entityBehaviors != null ? entityBehaviors.Behaviors.ToList()
                : new List<Behavior>();

            // Add any general factory behaviors for which there is not already an entity specific behavior.
            foreach (var factoryBehavior in _factoryBehaviors)
            {
                Type behaviorType = factoryBehavior.Key;

                // Factory behaviors are 1..1 since the key is the behavior type and not the entity type.
                Behavior behavior = factoryBehavior.Value.Behaviors.First();  

                if (!supportedBehaviors.Any(b => b.BehaviorType == behaviorType))
                {
                    supportedBehaviors.Add(behavior);
                }
            }

            // For each supported behavior, create an object containing the behavior type information
            // that will be used to store the behavior instance for the entity when first accessed.
            return supportedBehaviors.Select(b => new EntityBehavior(b));
        }

        public void BehaviorsFor<TDomainEntity>(Action<ISupportedBehaviors> entity) 
            where TDomainEntity : IBehaviorDelegator
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var domainEntityType = typeof(TDomainEntity);

            // Lookup the behavior registrations for a given entity type.  If not present,
            // create new instance.
            if (!_entityBehaviors.TryGetValue(domainEntityType, out SupportedBehaviors behaviors))
            {
                behaviors = new SupportedBehaviors(domainEntityType);
                _entityBehaviors[domainEntityType] = behaviors;
            }

            // Allow the caller to registered entity supported behaviors.
            entity(behaviors);
        }

        public IFactoryRegistry AddBehavior<TBehavior, TImplementation>()
           where TBehavior : IDomainBehavior
           where TImplementation : TBehavior
        {
            Type behaviorType = typeof(TBehavior);

            if (_factoryBehaviors.ContainsKey(behaviorType))
            {
                throw new InvalidOperationException(
                    $"The behavior of type {behaviorType.FullName} is already registered.");
            }

            var behaviors = new SupportedBehaviors();
            behaviors.Add<TBehavior, TImplementation>();

            _factoryBehaviors[behaviorType] = behaviors;
            return this;
        }
    }
}
