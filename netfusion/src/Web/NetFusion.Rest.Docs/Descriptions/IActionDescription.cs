using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Descriptions
{
    public interface IActionDescription
    {
        void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta);
    }
}