using System.Net.Http;
using System.Threading.Tasks;
using Demo.WebApi.Controllers;
using Demo.WebApi.Resources;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Common;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Hal;

namespace Demo.WebApi.HalMappings
{
    #pragma warning disable CS4014
    namespace ApiHost.Relations
    {
        public class ResourceMappings : HalResourceMap
        {
            protected override void OnBuildResourceMap()
            {
                // *** Uncomment for the expression example ***
                /*
                   Map<ListingModel>()    
                    .LinkMeta<ListingController>(meta =>
                    {
                        meta.Url(RelationTypes.Self, (c, r) => c.GetListing(r.ListingId));
                        meta.Url("listing:update", (c, r) => c.UpdateListing(r.ListingId, null));
                        meta.Url("listing:delete", (c, r) => c.DeleteListing(r.ListingId));
                    })

                    .LinkMeta<PriceHistoryController>(meta => {
                        meta.Url(RelationTypes.History.Archives, (c, r) => c.GetPriceHistoryEvents(r.ListingId));
                    });
                */
                
                // -----------------------------------------------------------------------------------------------
                
                // *** Uncomment for the hard-coded URL example ***
                /*
                
                 Map<ListingModel>()
                    .LinkMeta(meta => meta.Href("conn", HttpMethod.Get, "https://www.realtor.com/propertyrecord-search/Connecticut"))
                    .LinkMeta(meta => meta.Href("conn-cheshire", HttpMethod.Get, "https://www.realtor.com/realestateandhomes-search/Cheshire_CT"));
                
                */
                
                // -----------------------------------------------------------------------------------------------
                
                // *** Uncomment for the resource string interpolated example ***
                /*
                
                 Map<ListingModel>()
                    .LinkMeta(meta => meta.Href(RelationTypes.Alternate, HttpMethod.Get, r => $"http://www.homes.com/for/sale/{r.ListingId}"));
                
                */
                
                // -----------------------------------------------------------------------------------------------
               
                // *** Uncomment for the template URL example ***
                /*
                
                 Map<ListingModel>()
                    .LinkMeta<PriceHistoryController>(meta => meta.UrlTemplate<int, Task<IActionResult>>(
                        "listing:prices", c => c.GetPriceHistoryEvents));
                
                */

                // -----------------------------------------------------------------------------------------------
                
                // *** Uncomment for embedded resource example ***
                
                 
                  Map<ListingModel>()
                    .LinkMeta<ListingController>(meta =>
                    {
                        meta.Url(RelationTypes.Self, (c, r) => c.GetListing(r.ListingId));
                        meta.Url("listing:update", (c, r) => c.UpdateListing(r.ListingId, null));
                        meta.Url("listing:delete", (c, r) => c.DeleteListing(r.ListingId));
                    });
                
                Map<PriceHistoryModel>()
                    .LinkMeta<PriceHistoryController>(meta =>
                    {
                        meta.Url(RelationTypes.Self, (c, r) => c.GetPriceHistory(r.PriceHistoryId));
                        meta.Url("Events", (c, r) => c.GetPriceHistoryEvents(r.ListingId));
                    });
                
            }
        }
    }
}

