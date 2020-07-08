using System.Collections.Generic;
using System.Linq;
using NetFusion.Rest.Common;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Server.Linking;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    /// <summary>
    /// Determines if there are any resource links associated with the
    /// resources returned from the WebApi.  If so, documentation of
    /// the relations are added to the resource's document.
    /// </summary>
    public class XmlRelationComments : ILinkedDescription
    {
        private readonly IResourceMediaModule _resourceMediaModule;
        private readonly IApiMetadataService _metadataService;
        private readonly IXmlCommentService _xmlComments;

        public XmlRelationComments(IResourceMediaModule resourceMediaModule,
            IApiMetadataService metadataService,
            IXmlCommentService xmlComments)
        {
            _resourceMediaModule = resourceMediaModule;
            _metadataService = metadataService;
            _xmlComments = xmlComments;
        }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            var (entry, ok) = _resourceMediaModule.GetMediaTypeEntry(InternetMediaTypes.HalJson);
            if (!ok)
            {
                return;
            }

            foreach(ApiResourceDoc resourceDoc in actionDoc.ResponseDocs
                .Where(rd => rd.ResourceDoc != null)
                .Select(rd => rd.ResourceDoc))
            {
                ApplyRelationDocs(resourceDoc, entry);
            }
        }


        private void ApplyRelationDocs(ApiResourceDoc resourceDoc, MediaTypeEntry mediaTypeEntry)
        {
            var (meta, ok) = mediaTypeEntry.GetResourceTypeMeta(resourceDoc.ResourceType);
            if (ok)
            {
                var relationDocs = new List<ApiRelationDoc>();

                foreach(ResourceLink resourceLink in meta.Links)
                {
                    var relationDoc = new ApiRelationDoc
                    {
                        Name = resourceLink.RelationName,
                        Method = resourceLink.Methods.FirstOrDefault(),
                    };

                    SetRelationInfo(relationDoc, (dynamic)resourceLink);
                    relationDocs.Add(relationDoc);

                }

                resourceDoc.RelationDocs = relationDocs.ToArray();
            }

            // Next check any embedded resources:
            foreach (ApiResourceDoc embeddedResourceDoc in resourceDoc.EmbeddedResources
                .Select(er => er.ResourceDoc))
            {
                ApplyRelationDocs(embeddedResourceDoc, mediaTypeEntry);
            }
        }


        private static void SetRelationInfo(ApiRelationDoc relationDoc, ResourceLink resourceLink)
        {
            relationDoc.HRef = resourceLink.Href;
        }

        private static void SetRelationInfo(ApiRelationDoc relationDoc, InterpolatedLink resourceLink)
        {
           
        }

        private void SetRelationInfo(ApiRelationDoc relationDoc, ControllerActionLink resourceLink)
        {
            relationDoc.HRef = _metadataService.GetActionMeta(resourceLink.ActionMethodInfo).RelativePath;
            relationDoc.Description = _xmlComments.GetMethodComments(resourceLink.ActionMethodInfo);
        }

        private void SetRelationInfo(ApiRelationDoc relationDoc, TemplateUrlLink resourceLink)
        {
            relationDoc.HRef = _metadataService.GetActionMeta(resourceLink.ActionMethodInfo).RelativePath;
            relationDoc.Description = _xmlComments.GetMethodComments(resourceLink.ActionMethodInfo);
        }
    }
}
