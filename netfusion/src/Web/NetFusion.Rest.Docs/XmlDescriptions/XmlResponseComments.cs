using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlResponseComments : IResponseDescription
    {
        public DescriptionContext Context { get; set; }

        private readonly ITypeCommentService _typecomments;

        public XmlResponseComments(ITypeCommentService typeComments)
        {
            _typecomments = typeComments;
        }
        
        public void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta)
        {
            ApiResourceDoc resourceDoc = _typecomments.GetResourceDoc(responseMeta.ModelType);
            responseDoc.ResourceDocs.Add(resourceDoc);
        }
    }
}