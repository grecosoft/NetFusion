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
    /// Recursively processes all resource response documents specified for a given Api
    /// Response document.  For each possible response resource, a check is determined 
    /// if there are any documented embedded resources.
    /// </summary>
    public class XmlEmbeddedComments : IEmbeddedDescription
    {
        private readonly ITypeCommentService _typeComments;

        public XmlEmbeddedComments(ITypeCommentService typeComments)
        {
            _typeComments = typeComments ?? throw new System.ArgumentNullException(nameof(typeComments));
        }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            // Process all resource documents accross all response documents.
            foreach (ApiResourceDoc resourceDoc in actionDoc.ResponseDocs
                .Where(d => d.ResourceDoc != null)
                .Select(d => d.ResourceDoc))
            {
                ApplyEmbeddedResourceDocs(resourceDoc, actionDoc.EmbeddedResourceAttribs);
            }
        }

        private void ApplyEmbeddedResourceDocs(ApiResourceDoc resourceDoc,
            EmbeddedResourceAttribute[] embeddedResources)
        {
            resourceDoc.EmbeddedResources = GetEmbeddedResourceDocs(resourceDoc, embeddedResources).ToArray();

            // Next recursively process any embedded documents to determine if they
            // have any embedded children resources.
            foreach(ApiResourceDoc embeddedResourceDoc in resourceDoc.EmbeddedResources
                .Select(er => er.ResourceDoc))
            {
                ApplyEmbeddedResourceDocs(embeddedResourceDoc, embeddedResources);
            }
        }

        private IEnumerable<ApiEmbeddedDoc> GetEmbeddedResourceDocs(ApiResourceDoc parentResourceDoc,
            EmbeddedResourceAttribute[] embeddedResources)
        {
            // Find any embedded types specified for the resource type.
            var resourceEmbeddedTypes = embeddedResources.Where(et =>
                et.ParentResourceType.GetExposedResourceName() == parentResourceDoc.ResourceName);

            // For each embedded resource type, create an embedded resource document
            // with the documentation for each child embedded resource.
            foreach (EmbeddedResourceAttribute resourceEmbeddedType in resourceEmbeddedTypes)
            {
                var embeddedResourceDoc = new ApiEmbeddedDoc
                {
                    EmbeddedName = resourceEmbeddedType.EmbeddedName,
                    IsCollection = resourceEmbeddedType.IsCollection,
                    ResourceDoc = _typeComments.GetResourceDoc(resourceEmbeddedType.ChildResourceType)
                };

                embeddedResourceDoc.ResourceDoc.ResourceName = resourceEmbeddedType
                    .ChildResourceType.GetExposedResourceName();

                yield return embeddedResourceDoc;
            }
        }
    }
}
