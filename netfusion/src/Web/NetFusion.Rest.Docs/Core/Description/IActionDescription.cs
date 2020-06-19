using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Core.Description
{
    public interface IActionDescription : IDocDescription
    {
        void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta);
    }
}