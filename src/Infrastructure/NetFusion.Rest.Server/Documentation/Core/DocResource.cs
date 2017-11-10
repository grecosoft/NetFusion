using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Resources.Doc;
using NetFusion.Rest.Server.Resources;
using System;
using System.Linq;
using System.Reflection;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Documentation for a specific resource type.  This includes a description
    /// of each of the resource's properties.
    /// </summary>
    public class DocResource
    {
        public string Name { get; }
        public string Description { get; }
        public DocResourceProp[] Properties { get; }

        public DocResource(Type resourceType, DocResourceAttribute resourceDoc)
        {
            Name = resourceType.GetEmbeddedTypeName();
            Description = resourceDoc.Description;
            Properties = GetPropertyDoc(resourceType);
        }

        private DocResourceProp[] GetPropertyDoc(Type resourceType)
        {
            return resourceType.GetProperties()
                .Select(rp => new DocResourceProp(rp)
                {
                    Description = rp.GetAttribute<DocPropertyAttribute>()?.Description

                }).ToArray();
        }
    }
}
