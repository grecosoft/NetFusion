using System;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Represents a definition for a behavior that can be supported by an entity.
    /// </summary>
    public class Behavior : IBehavior
    {
        public Type ContractType { get; }
        public Type BehaviorType { get; }

        public Behavior(Type contractType, Type behaviorType)
        {
            this.ContractType = contractType;
            this.BehaviorType = behaviorType;
        }
    }
}
