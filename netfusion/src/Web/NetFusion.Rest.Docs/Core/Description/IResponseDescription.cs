using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Description
{
    public interface IResponseDescription : IDocDescription
    {
        void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta);
    }
}