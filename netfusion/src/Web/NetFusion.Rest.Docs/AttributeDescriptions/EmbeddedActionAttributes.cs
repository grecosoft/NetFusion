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
            var attributes = actionMeta.GetFilterMetadata<EmbeddedResourceAttribute>();
            if (!attributes.Any())
            {
                return;
            }

            // (example:  'payment-type' could be embeeded for resource names:
            // 'last-payment' and 'average-payment'.
            actionDoc.EmbeddedTypes = attributes.GroupBy(ea => ea.EmbeddedType)
                .Select(et =>
                    new EmbeddedType(
                        et.Key,
                        et.Select(ea => ea.EmbeddedName)))
                .ToArray();

        }
    }
}