using NetFusion.Common;
using NetFusion.Domain.Entities.Registration;
using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Behaviors supported by a type of domain-entity.
    /// </summary>
    public class SupportedBehavior : ISupportedBehavior
    {
        public Type DomainEntityType { get; }
        public Dictionary<Type, Behavior> _behaviors = new Dictionary<Type, Behavior>();

        public SupportedBehavior()
        {

        }

        public SupportedBehavior(Type domainEntityType)
        {
            Check.NotNull(domainEntityType, nameof(domainEntityType));
            this.DomainEntityType = domainEntityType;
        }

        public IEnumerable<Behavior> SupportedBehaviors => _behaviors.Values;

        public ISupportedBehavior Supports<TContract, TBehavior>() where TBehavior : TContract
        {
            Type behaviorType = typeof(TContract);
            Behavior behaviorReg = null;

            if (_behaviors.TryGetValue(behaviorType, out behaviorReg))
            {
                throw new InvalidOperationException(
                    $"The behavior of type: {behaviorType} is already registered with the domain-entity of type: {DomainEntityType}");
            }

            _behaviors[behaviorType] = new Behavior(behaviorType, typeof(TBehavior));
            return this;
        }
    }
}
