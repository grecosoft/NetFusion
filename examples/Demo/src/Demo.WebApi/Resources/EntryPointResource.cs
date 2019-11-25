using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Hal;
using System.Threading.Tasks;
using Demo.WebApi.Controllers;

#pragma warning disable CS4014
namespace Demo.WebApi.Resources
{
    public class EntityPointResourceMap : HalResourceMap
    {
        protected override void OnBuildResourceMap()
        {
            Map<HalEntryPointResource>()
                .LinkMeta<ListingController>(meta =>
                {

                    meta.UrlTemplate<int, Task<ListingResource>>("listing:entry", c => c.GetListing);
                })
                .LinkMeta<PriceHistoryController>(meta =>
                {
                    meta.UrlTemplate<int, Task<PriceHistoryResource>>("history:entry", c => c.GetPriceHistory);
                });
        }
    }
}
