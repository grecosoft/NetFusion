using Demo.Api.Resources;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Hal;
using System.Threading.Tasks;
using Demo.WebApi.Controllers;

#pragma warning disable CS4014
namespace Demo.WebApi.Resources
{
    public class EntryPointResource : HalResource
    {
    }

    public class EntityPointResourceMap : HalResourceMap
    {
        public override void OnBuildResourceMap()
        {
            Map<EntryPointResource>()
                .LinkMeta<ListingController>(meta =>
                {

                    meta.UrlTemplate<int, Task<ListingResource>>("listing:entry", c => c.GetListing);
                    meta.UrlTemplate<int, Task<ListingSummaryResource>>("listing:summary", c => c.GetListingSummary);
                })
                .LinkMeta<PriceHistoryController>(meta =>
                {
                    meta.UrlTemplate<int, Task<PriceHistoryResource>>("history:entry", c => c.GetPriceHistory);
                });
        }
    }
}
