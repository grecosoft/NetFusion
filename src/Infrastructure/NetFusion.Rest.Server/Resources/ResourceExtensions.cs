using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using System;

namespace NetFusion.Rest.Server.Resources
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// Indicates if the derived IResource type can be returned to the client.
        /// Resource collections should only be returned as embedded resources of
        /// a parent resource.
        /// </summary>
        /// <param name="resourceType">The resource type to check.</param>
        /// <returns>True if the resource can be returned.  Otherwise False.</returns>
        public static bool IsReturnableResourceType(this Type resourceType)
        {
            return resourceType.IsDerivedFrom<IResource>() &&
                !resourceType.IsClosedGenericTypeOf(typeof(IResourceCollection<>));
        }

        /// <summary>
        /// Returns the embedded name associated with a given resource type used to
        /// identity the resource to clients.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <returns>The embedded name specified by the NamedResource attribute.  If the
        /// attribute is not present, null is returned.</returns>
        public static string GetEmbeddedTypeName(this Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType),
                "Resource Type cannot be null.");

            return resourceType.GetAttribute<NamedResourceAttribute>()?.ResourceName;
        }
    }
}
