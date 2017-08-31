using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources;
using System;

namespace NetFusion.Rest.Server.Resources.Core
{
    public static class ResourceExtenions
    {
        public static bool IsReturnableResourceType(this Type resourceType)
        {
            return resourceType.IsDerivedFrom<IResource>() &&
                !resourceType.IsClosedGenericTypeOf(typeof(IResourceCollection<>));
        }

        public static string GetEmbeddedName(this Type resourceType)
        {
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType), "Resource Type not specified.");

            return resourceType.GetAttribute<NamedResourceAttribute>()?.ResourceName;
        }
    }
}
