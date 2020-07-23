using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Xml.Descriptions
{
    /// <summary>
    /// Adds resource documentation to responses resource.
    /// </summary>
    public class XmlResponseComments : IResponseDescription
    {
        private readonly ITypeCommentService _typeComments;

        public XmlResponseComments(ITypeCommentService typeComments)
        {
            _typeComments = typeComments ?? throw new System.ArgumentNullException(nameof(typeComments));
        }
        
        public void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta)
        {
            responseDoc.ResourceDoc = _typeComments.GetResourceDoc(responseMeta.ModelType);
        }
    }
}