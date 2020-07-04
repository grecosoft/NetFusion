using System.Linq;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;
using System.Collections.Generic;
using NetFusion.Rest.Docs.Core;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    /// <summary>
    /// Recursively processes all resource response documents specified for a given
    /// Api Response document.  
    /// </summary>
    public class XmlEmbeddedComments : IEmbeddedDescription
    {
        private readonly ITypeCommentService _typeComments;
        public DescriptionContext Context { get; set; }

        public XmlEmbeddedComments(ITypeCommentService typeComments)
        {
            _typeComments = typeComments ?? throw new System.ArgumentNullException(nameof(typeComments));
        }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            // Process all resource documents accross all response documents.
            foreach (ApiResourceDoc resourceDoc in actionDoc.ResponseDocs
                .SelectMany(d => d.ResourceDocs))
            {
                ApplyEmbeddedResourceDocs(resourceDoc, actionDoc.EmbeddedTypes);
            }
        }

        private void ApplyEmbeddedResourceDocs(ApiResourceDoc resourceDoc,
            EmbeddedType[] embeddedTypes)
        {
            resourceDoc.EmbeddedResources = GetEmbeddedResourceDocs(resourceDoc, embeddedTypes).ToArray();

            // Recursively process all child resource documents for properties have a type
            // of a resource.
            foreach (ApiResourceDoc childResourceDoc in resourceDoc.Properties
                .Select(rd => rd.Type).OfType<ApiResourceDoc>())
            {
                ApplyEmbeddedResourceDocs(childResourceDoc, embeddedTypes);
            }
        }

        private IEnumerable<ApiEmbeddedDoc> GetEmbeddedResourceDocs(ApiResourceDoc parentResourceDoc,
            EmbeddedType[] embeddedTypes)
        {
            // Find any embedded types specified for the resource type.
            var resourceEmbeddedTypes = embeddedTypes.Where(et =>
                et.ParentModelType.GetExposedResourceName() == parentResourceDoc.ResourceName);

            // For each embedded resource type, create an embedded resource document
            // with the documentation for each child embedded resource.
            foreach (EmbeddedType resourceEmbeddedType in resourceEmbeddedTypes)
            {
                var embeddedResourceDoc = new ApiEmbeddedDoc
                {
                    EmbeddedName = resourceEmbeddedType.EmbeddedName,
                    ResponseDoc = _typeComments.GetResourceDoc(resourceEmbeddedType.ChildModelType)
                };

                embeddedResourceDoc.ResponseDoc.ResourceName = resourceEmbeddedType
                    .ChildModelType.GetExposedResourceName();

                yield return embeddedResourceDoc;
            }
        }
    }
}
