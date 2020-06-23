using System.Collections.Generic;
using System.Linq;
using NetFusion.Rest.Docs.Attributes;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Models;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Docs.AttributeDescriptions
{
    public class EmbeddedActionAttributes : IActionDescription
    {
        public DescriptionContext Context { get; set; }
        public void Describe(ApiActionDoc actionDoc, ApiActionMeta actionMeta)
        {
            var attribute = actionMeta.GetFilterMetadata<EmbeddedResourceAttribute>().FirstOrDefault();
            if (attribute != null)
            {
                actionDoc.EmbeddedTypes = attribute.EmbeddedTypes;
            }
        }
    }
}