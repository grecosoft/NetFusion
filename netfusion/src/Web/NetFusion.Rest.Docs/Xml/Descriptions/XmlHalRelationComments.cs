using System;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Server.Linking;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Xml.Descriptions
{
    /// <summary>
    /// Adds additional documentation of a link relation existing between two resources.
    /// </summary>
    public class XmlHalRelationComments : IRelationDescription
    {
        private readonly IApiMetadataService _metadataService;
        private readonly IXmlCommentService _xmlComments;

        public XmlHalRelationComments(
            IApiMetadataService metadataService,
            IXmlCommentService xmlComments)
        {
            _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
            _xmlComments = xmlComments ?? throw new ArgumentNullException(nameof(xmlComments));
        }
        
        public void Describe(ApiResourceDoc resourceDoc, ApiRelationDoc relationDoc, ResourceLink resourceLink)
        {
            SetRelationInfo(relationDoc, (dynamic)resourceLink);
        }
        
        private static void SetRelationInfo(ApiRelationDoc relationDoc, ResourceLink resourceLink)
        {
            relationDoc.HRef = resourceLink.Href;
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
