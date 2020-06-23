using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.XmlDescriptions
{
    public class XmlResponseComments : IResponseDescription
    {
        public DescriptionContext Context { get; set; }
        
        public void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta)
        {
            ApiResourceDoc resourceDoc = Context.TypeComments.GetResourceDoc(responseMeta.ModelType);
            responseDoc.ResourceDocs.Add(resourceDoc);
        }
    }
}