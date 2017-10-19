using NetFusion.Common;
using System;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Represents a definition for a behavior that can be supported by an aggregate or entity.
    /// </summary>
    public class Behavior : IBehavior
    {
        public Type BehaviorType { get; }
        public Type ImplementationType { get; }

        // Indicates that the implementation type has one and only one constructor with a
        // parameter of a type deriving from IBehaviorDelegator.
        public bool HasDelegatorConstructor { get; }

        public Behavior(Type behaviorType, Type implementationType, bool hasDelegatorConstructor)
        {
            Check.NotNull(behaviorType, nameof(behaviorType));
            Check.NotNull(implementationType, nameof(implementationType));

            BehaviorType = behaviorType;
            ImplementationType = implementationType;
            HasDelegatorConstructor = hasDelegatorConstructor;
        }
    }
}
