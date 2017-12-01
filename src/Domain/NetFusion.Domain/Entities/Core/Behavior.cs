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
            BehaviorType = behaviorType ?? throw new ArgumentNullException(nameof(behaviorType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(hasDelegatorConstructor));

            HasDelegatorConstructor = hasDelegatorConstructor;
        }
    }
}
