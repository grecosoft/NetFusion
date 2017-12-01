﻿using System;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Represents a behavior instance associated with a specific entity.
    /// </summary>
    public class EntityBehavior
    {
        public Behavior Behavior { get; }
        public IDomainBehavior Instance { get; set; }

        public EntityBehavior(Behavior behavior)
        {           
            Behavior = behavior ?? throw new ArgumentNullException(nameof(behavior));
        }
    }
}
