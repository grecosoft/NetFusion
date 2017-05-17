using NetFusion.Common.Extensions.Reflection;
using NetFusion.Domain.Entities.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Delegated to by an instance of a domain entity for accessing its associated behaviors.
    /// </summary>
    public class Entity : IEntity
    {
        private IEntityDelegator _entityDelegator;
        private IDomainServiceResolver _resolver;

        private Lazy<IEnumerable<EntityBehavior>> _deferredBehaviors;
        private Dictionary<Type, EntityBehavior> _behaviors;

        public Entity(IEntityDelegator entityDelegator, 
            IDomainServiceResolver resolver,
            Lazy<IEnumerable<EntityBehavior>> deferredBehaviors)
        {
            _entityDelegator = entityDelegator;
            _resolver = resolver;
            _deferredBehaviors = deferredBehaviors;
        }

        private Dictionary<Type, EntityBehavior> Behaviors
        {
            get
            {
                if (_behaviors == null)
                {
                    _behaviors = _deferredBehaviors.Value.ToDictionary(e => e.Behavior.ContractType);
                }
                return _behaviors;
            }
        }

        public (TBehavior instance, bool supported) GetBehavior<TBehavior>() where TBehavior : class, IDomainBehavior
        {
            EntityBehavior behavior = null;

            if (!Behaviors.TryGetValue(typeof(TBehavior), out behavior))
            {
                return (null, false);
            }
    
            if (behavior.Instance == null)
            {
                behavior.Instance = CreateBehavior(behavior);
            }

            return ((TBehavior)behavior.Instance, true);
        }

        public bool SupportsBehavior<TBehavior>() where TBehavior : IDomainBehavior
        {
            return Behaviors.ContainsKey(typeof(TBehavior));
        }

        // Creates an instance of an entity-behavior and injects and needed domain services.
        private IDomainBehavior CreateBehavior(EntityBehavior entityBehavior)
        {
            if (entityBehavior.Instance == null)
            {
                Type behaviorType = entityBehavior.Behavior.BehaviorType;
                object[] constructorValues = GetConstructorValues(behaviorType);

                if (constructorValues.Length > 0)
                {
                    entityBehavior.Instance = (IDomainBehavior)Activator.CreateInstance(behaviorType, constructorValues);
                }
                else if (behaviorType.HasDefaultConstructor())
                {
                    entityBehavior.Instance = (IDomainBehavior)Activator.CreateInstance(behaviorType);
                }

                if (entityBehavior.Instance == null)
                {
                    throw new InvalidOperationException(
                        $"The domain-entity behavior of type: {behaviorType} could not be created.  " +
                        $"The type does not have a correct constructor signature.");
                }

                // Inject any domain-services needed by the behavior.
                _resolver.ResolveDomainServices(entityBehavior.Instance);
            }
            return entityBehavior.Instance;
        }

        private object[] GetConstructorValues(Type behaviorType)
        {
            IEnumerable<ConstructorInfo> constructors = behaviorType.GetTypeInfo()
                .GetConstructors()
                .Where(c => IsBehaviorConstructor(c));

            ConstructorInfo constructor = constructors.FirstOrDefault();

            if (constructor == null)
            {
                return new object[] { };
            }

            var paramValues = new List<object>();

            foreach(var paramInfo in constructor.GetParameters())
            {
                if (typeof(IEntityDelegator).IsAssignableFrom(paramInfo.ParameterType))
                {
                    paramValues.Add(_entityDelegator);
                    continue;
                }

                Type accessorType = GetAccessorType();
                if (accessorType != null)
                {
                    var accessor = Activator.CreateInstance(accessorType, _entityDelegator);
                    paramValues.Add(accessor);
                    continue;
                }
            }

            return paramValues.ToArray();
        }

        private Type GetAccessorType()
        {
            var entityAccessorType = _entityDelegator.GetType()
                .GetInterfaces()
                .FirstOrDefault(it => it.IsClosedGenericTypeOf(typeof(IEntityDelegator<>), typeof(IEntityAccessor)));

            if (entityAccessorType != null)
            {
                return entityAccessorType.GetGenericArguments().First();
            }

            return null;
        }

        private bool IsBehaviorConstructor(ConstructorInfo constructorInfo)
        {
            var constructorParamsTypes = constructorInfo.GetParameters().Select(p => p.ParameterType);
            var validParamTypes = new Type[] { typeof(IEntityDelegator), typeof(IEntityAccessor) };

            return constructorParamsTypes.All(
                cpt => validParamTypes.Any(vpt => vpt.IsAssignableFrom(cpt)));
        }
    }
}
