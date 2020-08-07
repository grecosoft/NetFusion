using System;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Server.Linking;

namespace NetFusion.Rest.Docs.Xml.Descriptions
{
    /// <summary>
    /// Adds additional documentation of a link relation existing between two resources
    /// from a .NET Code Comment XML file.
    /// </summary>
    public class XmlHalRelationComments : IRelationDescription
    {
        private readonly IXmlCommentService _xmlComments;

        public XmlHalRelationComments(
            IXmlCommentService xmlComments)
        {
            _xmlComments = xmlComments ?? throw new ArgumentNullException(nameof(xmlComments));
        }
        
        public void Describe(ApiResourceDoc resourceDoc, ApiRelationDoc relationDoc, ResourceLink resourceLink)
        {
            SetRelationInfo(relationDoc, (dynamic)resourceLink);
        }
        
        private void SetRelationInfo(ApiRelationDoc relationDoc, ControllerActionLink resourceLink)
        {
            relationDoc.Description = _xmlComments.GetMethodComments(resourceLink.ActionMethodInfo);
        }

        private void SetRelationInfo(ApiRelationDoc relationDoc, TemplateUrlLink resourceLink)
        {
            relationDoc.Description = _xmlComments.GetMethodComments(resourceLink.ActionMethodInfo);
        }
    }
}
