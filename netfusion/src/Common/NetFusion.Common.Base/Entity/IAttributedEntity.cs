﻿using System.Collections.Generic;

namespace NetFusion.Common.Base.Entity;

/// <summary>
/// Identifies an entity having a set of key/value attributes that
/// can be accessed dynamically at runtime.
/// </summary>
public interface IAttributedEntity
{
    /// <summary>
    /// Reference to object used to manage an entity's dynamic attributes.
    /// </summary>
    IEntityAttributes Attributes { get; }

    /// <summary>
    /// A dictionary of the key value pairs associated with the entity
    /// in which the dynamic properties are stored.
    /// </summary>
    IDictionary<string, object?> AttributeValues { get; set; }
}