using Demo.Api.Resources;
using NetFusion.Rest.Common;
using NetFusion.Rest.Server.Hal;
using Demo.WebApi.Controllers;
using System.Net.Http;
using System.Threading.Tasks;
using NetFusion.Rest.Resources.Hal;

namespace Demo.WebApi.HalMappings
{
    #pragma warning disable CS4014
    namespace ApiHost.Relations
    {
        public class ResourceMappings : HalResourceMap
        {
            public override void OnBuildResourceMap()
            {
                // ...........................................................
                // For Example: Controller/Resource Lambda Expression
                // ...........................................................

                // Map<ListingResource>()
                //     .LinkMeta<ListingController>(meta =>
                //     {
                //         meta.Url(RelationTypes.Self, (c, r) => c.GetListing(r.ListingId));
                //         meta.Url("listing:update", (c, r) => c.UpdateListing(r.ListingId, default(ListingResource)));
                //         meta.Url("listing:delete", (c, r) => c.DeleteListing(r.ListingId));
                //     })

                //     .LinkMeta<PriceHistoryController>(meta => {
                //         meta.Url(RelationTypes.History.Archives, (c, r) => c.GetPriceHistoryEvents(r.ListingId));
                //     });

                // ...........................................................
                // For Example: Hard-Coding URL String
                // ...........................................................

                // Map<ListingResource>()
                //     .LinkMeta(meta => meta.Href("conn", HttpMethod.Get, "https://www.realtor.com/propertyrecord-search/Connecticut"))
                //     .LinkMeta(meta => meta.Href("conn-cheshire", HttpMethod.Get, "https://www.realtor.com/realestateandhomes-search/Cheshire_CT"));

                // ...........................................................
                // For Example: Resource String Interpolated URL String
                // ...........................................................

                // Map<ListingResource>()
                //     .LinkMeta(meta => meta.Href(RelationTypes.Alternate, HttpMethod.Get, r => $"http://www.homes.com/for/sale/{r.ListingId}"));

                // ...........................................................
                // For Example: URL Templates
                // ...........................................................

                // Map<ListingResource>()
                //     .LinkMeta<PriceHistoryController>(meta => meta.UrlTemplate<int, Task<HalResource>>(
                //         "listing:prices", c => c.GetPriceHistoryEvents));

                // ...........................................................
                // For Example: Auto Mapping
                // ...........................................................
                // Map<ListingResource>()
                //     .LinkMeta<ListingController>(meta => {
                //         meta.AutoMapSelfRelation();
                //         meta.AutoMapUpdateRelations();
                // });

                // ...........................................................
                // For Example: Shared Mappings
                // ...........................................................

                // Map<ListingResource>()
                //     .LinkMeta<ListingController>(meta =>
                //     {
                //         meta.Url(RelationTypes.Self, (c, r) => c.GetListing(r.ListingId));
                //         meta.Url("listing:update", (c, r) => c.UpdateListing(r.ListingId, null));
                //         meta.Url("listing:delete", (c, r) => c.DeleteListing(r.ListingId));
                //     })

                //     .LinkMeta<PriceHistoryController>(meta =>
                //     {
                //         meta.Url(RelationTypes.History.Archives, (c, r) => c.GetPriceHistoryEvents(r.ListingId));
                //     });

                // Map<ListingSummaryResource>()
                //     .ApplyLinkMetaFrom<ListingResource>();

                // ...........................................................
                // For Example: Embedded Resources
                // ...........................................................
                
                // Map<ListingResource>()
                //     .LinkMeta<ListingController>(meta =>
                //     {
                //         meta.Url(RelationTypes.Self, (c, r) => c.GetListing(r.ListingId));
                //         meta.Url("listing:update", (c, r) => c.UpdateListing(r.ListingId, null));
                //         meta.Url("listing:delete", (c, r) => c.DeleteListing(r.ListingId));
                //     })

                //     .LinkMeta<PriceHistoryController>(meta =>
                //     {
                //         meta.Url(RelationTypes.History.Archives, (c, r) => c.GetPriceHistoryEvents(r.ListingId));
                //     });

                // Map<PriceHistoryResource>()
                //     .LinkMeta<PriceHistoryController>(meta => {
                //         meta.AutoMapSelfRelation();
                //     });

            }
        }
    }
}
