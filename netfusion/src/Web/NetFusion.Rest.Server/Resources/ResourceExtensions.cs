using NetFusion.Common.Extensions.Reflection;
using System;

namespace NetFusion.Rest.Server.Resources
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// Returns the embedded name associated with a given resource type used to
        /// identity the resource to clients.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <returns>The embedded name specified by the NamedResource attribute.  If the
        /// attribute is not present, null is returned.</returns>
        public static string GetExposedResourceTypeName(this Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType),
                "Resource Type cannot be null.");

            return resourceType.GetAttribute<ExposedResourceNameAttribute>()?.ResourceName;
        }
    }
}
