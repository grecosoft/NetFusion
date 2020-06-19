using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Description
{
    public interface IParamDescription : IDocDescription
    {
        void Describe(ApiParameterDoc paramDoc, ApiParameterMeta paramMeta);
    }
}