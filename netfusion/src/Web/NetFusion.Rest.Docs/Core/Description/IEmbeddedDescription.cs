using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Description
{
    public interface IEmbeddedDescription : IDocDescription
    {
        void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta);
    }
}
