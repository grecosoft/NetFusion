using NetFusion.Common.Extensions.Reflection;
using NetFusion.Domain.Entities.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Behaviors supported by a type of domain-entity or aggregate.
    /// </summary>
    public class SupportedBehaviors : ISupportedBehaviors
    {
        public Type DomainEntityType { get; }
        public Dictionary<Type, Behavior> _behaviors = new Dictionary<Type, Behavior>();

        // Constructor for creating factory-level supported behavior.
        public SupportedBehaviors()
        {

        }

        // Constructor for creating a domain entity level supported behaviors.
        public SupportedBehaviors(Type domainEntityType)
        {
            DomainEntityType = domainEntityType ?? throw new ArgumentNullException(nameof(domainEntityType));
        }

        public IEnumerable<Behavior> Behaviors => _behaviors.Values;

        public ISupportedBehaviors Add<TBehavior, TImplementation>()
            where TBehavior : IDomainBehavior
            where TImplementation : TBehavior
        {
            Type behaviorType = typeof(TBehavior);
            Type implementationType = typeof(TImplementation);

            var behaviorConstructor = GetConstructorInfo(implementationType);
            if (behaviorConstructor.hasDefault || behaviorConstructor.singleDelegatorParam)
            {
                AddBehavior(behaviorType, implementationType, behaviorConstructor.singleDelegatorParam);
                return this;
            }

            throw new InvalidOperationException(
                $"The class: {implementationType.FullName} implementing the behavior type: {behaviorType.FullName} " +
                $"does not have a valid constructor.  A behavior type must have a default constructor or a single " +
                $"parameter constructor with a type implementing the: {typeof(IBehaviorDelegator).FullName} interface.");
        }

        private void AddBehavior(Type behaviorType, Type implementationType, bool hasDelegatorConstructor)
        {
            if (!_behaviors.ContainsKey(behaviorType))
            {
                _behaviors[behaviorType] = new Behavior(behaviorType, implementationType, hasDelegatorConstructor);
                return;
            }

            if (DomainEntityType != null)
            {
                throw new InvalidOperationException(
                    $"The behavior of type: {behaviorType} is already registered with the domain-entity of type: {DomainEntityType}");
            }

            throw new InvalidOperationException(
                $"The behavior of type: {behaviorType} is already registered.");
        }

        private (bool hasDefault, bool singleDelegatorParam) GetConstructorInfo(Type behaviorType)
        {
            var constructors = behaviorType.GetTypeInfo().GetConstructors()
                .Where(c => c.GetParameters().Length == 1);

            var singleParamConstructorTypes = constructors.SelectMany(c => c.GetParameters()
                .Select(p => p.ParameterType));

            return (
                behaviorType.HasDefaultConstructor(),
                singleParamConstructorTypes.Count(pt => typeof(IBehaviorDelegator).IsAssignableFrom(pt)) == 1);
        }
    }
}
