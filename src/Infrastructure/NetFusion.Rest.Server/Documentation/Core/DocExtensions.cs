using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources.Doc;
using NetFusion.Rest.Server.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Extensions for reading documentation specified with attributes.
    /// </summary>
    public static class DocExtensions
    {
        /// <summary>
        /// Returns the embedded resource names specified on an action method using the
        /// DocEmbeddedResource attribute.  These are a list of key values used to 
        /// identify embedded resources to the consuming client.
        /// </summary>
        /// <param name="actionMethodInfo">The action method to retrieve the list of embedded names.</param>
        /// <returns>List of embedded names.</returns>
        public static ActionEmbeddedName[] GetEmbeddedNames(this MethodInfo actionMethodInfo)
        {
            if (actionMethodInfo == null) throw new ArgumentNullException(nameof(actionMethodInfo),
                "Action Method cannot be null.");

            var specifiedEmbeddedNames = GetEmbeddedSpecifiedNames(actionMethodInfo);
            var specifiedEmbeddedTypes = GetEmbeddedSpecifiedTypes(actionMethodInfo);
            return specifiedEmbeddedNames.Union(specifiedEmbeddedTypes).ToArray(); 
        }

        // The case where the DocEmbeddedResource attribute was used to specify the embedded resource as string.
        private static IEnumerable<ActionEmbeddedName> GetEmbeddedSpecifiedNames(MethodInfo actionMethodInfo)
        {
            return actionMethodInfo.GetCustomAttributes<DocEmbeddedResourceAttribute>()
                .Where(attrib => attrib.EmbeddedResourceNames != null)
                .SelectMany(attrib => attrib.EmbeddedResourceNames)
                .Select(name => new ActionEmbeddedName { Name = name });
        }

        // The case were the DocEmbeddedResource attribute as used to specify the embedded resource by its type.
        private static IEnumerable<ActionEmbeddedName> GetEmbeddedSpecifiedTypes(MethodInfo actionMethodInfo)
        {
            return actionMethodInfo.GetCustomAttributes<DocEmbeddedResourceAttribute>()
                .Where(attrib => attrib.EmbeddedResourceTypes != null)
                .SelectMany(attrib => attrib.EmbeddedResourceTypes)
                .Where(t => t.IsReturnableResourceType())
                .Select(rt => new ActionEmbeddedName
                {
                    Name = rt.GetEmbeddedTypeName(),
                    ResourceType = rt,  // Return resource type so consumer can use to determine documentation.

                }).Where(en => en.Name != null);
        }        

        public static string GetResourceDescription(this Type resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType),
                "Resource Type cannot be null.");

            return resourceType.GetAttribute<DocResourceAttribute>()?.Description;
        }
    }
}
