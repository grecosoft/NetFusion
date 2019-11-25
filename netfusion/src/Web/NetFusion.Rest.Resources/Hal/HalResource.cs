using System.Collections.Generic;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Base class implementing the IHalResource interface from which resources can be derived.
    /// </summary>
    public class HalResource : IHalResource
    {
        /// <summary>
        /// List of links populated based on the configured resource metadata.
        /// </summary>
        public IDictionary<string, Link> Links { get; set; }

        /// <summary>
        /// Named embedded resources.
        /// </summary>
        public IDictionary<string, IResource> Embedded { get; set; }
    }
}
