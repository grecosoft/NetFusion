﻿using System;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Used to map a string name to a given resource model.  This attribute
    /// specifies a name used to identity the resource to clients consuming
    /// the resource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposedNameAttribute : Attribute
    {
        public string ResourceName { get; }

        public ExposedNameAttribute(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource name not specified.", nameof(resourceName));

            ResourceName = resourceName;
        }
    }
}
