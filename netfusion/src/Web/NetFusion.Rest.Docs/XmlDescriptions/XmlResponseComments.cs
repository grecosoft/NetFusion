using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlResponseComments : IResponseDescription
    {
        private readonly ITypeCommentService _typeComments;

        public XmlResponseComments(ITypeCommentService typeComments)
        {
            _typeComments = typeComments;
        }
        
        public void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta)
        {
            ApiResourceDoc resourceDoc = _typeComments.GetResourceDoc(responseMeta.ModelType);
            responseDoc.ResourceDocs.Add(resourceDoc);
        }
    }
}