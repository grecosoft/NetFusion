using System;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Resource that wraps another object that is not a resource.
    /// </summary>
    public class ObjectResource : IResource
    {
        /// <summary>
        /// The wrapped object value.
        /// </summary>
        public object Value { get; }
        
        /// <summary>
        /// Wraps an object that is not defined as a resource.
        /// </summary>
        /// <param name="value">The object being wrapped.</param>
        public ObjectResource(object value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}