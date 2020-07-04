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
    public class XmlRelationComments : ILinkedDescription
    {
        public DescriptionContext Context { get; set; }

        private readonly IResourceMediaModule _resourceMediaModule;
        private readonly IApiMetadataService _metadataService;

        public XmlRelationComments(IResourceMediaModule resourceMediaModule, IApiMetadataService metadataService)
        {
            _resourceMediaModule = resourceMediaModule;
            _metadataService = metadataService;
        }

        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            var (entry, ok) = _resourceMediaModule.GetMediaTypeEntry(InternetMediaTypes.HalJson);
            if (!ok)
            {
                return;
            }

            foreach(ApiResourceDoc resourceDoc in actionDoc.ResponseDocs.SelectMany(rd => rd.ResourceDocs))
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

            foreach (ApiResourceDoc embeddedResourceDoc in resourceDoc.EmbeddedResources.Select(er => er.ResponseDoc))
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
        }

        private void SetRelationInfo(ApiRelationDoc relationDoc, TemplateUrlLink resourceLink)
        {
            relationDoc.HRef = _metadataService.GetActionMeta(resourceLink.ActionMethodInfo).RelativePath;
        }
    }
}
