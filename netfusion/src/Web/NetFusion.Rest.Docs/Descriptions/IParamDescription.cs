using NetFusion.Rest.Docs.Core;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Descriptions
{
    public interface IParamDescription : IDocDescription
    {
        void Describe(ApiParameterDoc paramDoc, ApiParameterMeta paramMeta);
    }
}