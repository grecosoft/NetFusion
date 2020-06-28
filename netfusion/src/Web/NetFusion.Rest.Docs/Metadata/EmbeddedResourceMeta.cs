using System.Linq;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.Metadata
{

    public class EmbeddedResourceMeta : IActionDescription
    {
        public DescriptionContext Context { get; set; }
        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            var attributes = actionMeta.GetFilterMetadata<EmbeddedResourceAttribute>();
            if (!attributes.Any())
            {
                return;
            }

            actionDoc.EmbeddedTypes = attributes.Select(a => new EmbeddedType(
                a.ParentModelType,
                a.ChildModelType,
                a.EmbeddedName)).ToArray();

        }
    }
}