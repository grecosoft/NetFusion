using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Descriptions
{
    public interface IResponseDescription : IDocDescription
    {
        void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta);
    }
}