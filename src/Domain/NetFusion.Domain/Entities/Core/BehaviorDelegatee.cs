using NetFusion.Domain.Entities.Registration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Delegated to by an instance of a domain entity or aggregate for accessing its associated behaviors.
    /// The DomainEntityFactory, when creating an instance of a domain entity, creates and sets an instance
    /// of this class for the domain entity.
    /// </summary>
    public class BehaviorDelegatee : IBehaviorDelegatee
    {
        private IBehaviorDelegator _entityDelegator;
        private IDomainServiceResolver _resolver;

        private Lazy<IEnumerable<EntityBehavior>> _supportedBehaviors;
        private Dictionary<Type, EntityBehavior> _behaviors;

        public BehaviorDelegatee(IBehaviorDelegator behaviorDelegator,
            Lazy<IEnumerable<EntityBehavior>> supportedBehaviors,
            IDomainServiceResolver resolver)
        {
            _entityDelegator = behaviorDelegator;
            _supportedBehaviors = supportedBehaviors;
            _resolver = resolver;
        }

        // Creates a dictionary used to cache behavior instances upon first use.
        private Dictionary<Type, EntityBehavior> Behaviors
        {
            get
            {
                if (_behaviors == null)
                {
                    _behaviors = _supportedBehaviors.Value.ToDictionary(e => e.Behavior.BehaviorType);
                }
                return _behaviors;
            }
        }

        public bool Supports<TBehavior>() where TBehavior : IDomainBehavior
        {
            return Behaviors.ContainsKey(typeof(TBehavior));
        }

        public (TBehavior instance, bool supported) Get<TBehavior>() 
            where TBehavior : IDomainBehavior
        {
            if (!Behaviors.TryGetValue(typeof(TBehavior), out EntityBehavior entityBehavior))
            {
                return (default(TBehavior), false);
            }

            IDomainBehavior behaviorInstance = GetOrCreateBehavior(entityBehavior);
            return ((TBehavior)behaviorInstance, true);
        }

        public TBehavior GetRequired<TBehavior>() 
            where TBehavior : IDomainBehavior
        {
            var behavior = Get<TBehavior>();
            if (behavior.supported)
            {
                return behavior.instance;
            }

            throw new NotSupportedException(
                $"The entity of type: {_entityDelegator.GetType().FullName} does not support " +
                $" the behavior: {typeof(TBehavior).FullName}.");
        }

        // Creates an instance of an entity behavior and injects and needed domain services
        // using the resolver passed by the entity factory.
        private IDomainBehavior GetOrCreateBehavior(EntityBehavior entityBehavior)
        {
            if ((entityBehavior.Instance != null)) {
                return entityBehavior.Instance;
            }

            entityBehavior.Instance = CreateBehaviorInstance(entityBehavior.Behavior);

            // Inject any domain-services needed by the behavior.
            _resolver.ResolveDomainServices(entityBehavior.Instance);

            return entityBehavior.Instance;
        }

        private IDomainBehavior CreateBehaviorInstance(Behavior behavior)
        {
            if (behavior.HasDelegatorConstructor)
            {
                return (IDomainBehavior)Activator.CreateInstance(behavior.ImplementationType, _entityDelegator);
            }
            return (IDomainBehavior)Activator.CreateInstance(behavior.ImplementationType);
        }    
    }
}
