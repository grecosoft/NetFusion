using Demo.Api.Resources;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Web.Mvc.Metadata;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.WebApi.Controllers
{
    [Route("api/listing/price-history"), GroupMeta("Listing")]
    public class PriceHistoryController : Controller
    {

        [HttpGet("{id}"),
            ActionMeta("GetPriceHistory")]
        public Task<PriceHistoryResource> GetPriceHistory(int id)
        {
            var history = GetPricingHistory().FirstOrDefault(h => h.PriceHistoryId == id);
            return Task.FromResult(history);
        }

        [HttpGet("{listingId}/events"), ActionMeta("PriceHistory")]
        public Task<HalResource> GetPriceHistoryEvents(int listingId)
        {
            var items = GetPricingHistory().Where(h => h.ListingId == listingId);
            var historyResource = new HalResource();
            historyResource.Embed(items, "price-history");

            return Task.FromResult(historyResource);
        }

        private PriceHistoryResource[] GetPricingHistory()
        {
            return new PriceHistoryResource[] {
                new PriceHistoryResource {
                    ListingId = 1000,
                    DateOfEvent = DateTime.Parse("5/5/2016"),
                    PriceHistoryId = 2000,
                    Event = "Listed",
                    Price = 300_000,
                    Source = "SMARTMLS" },

                new PriceHistoryResource {
                    ListingId = 1000,
                    DateOfEvent = DateTime.Parse("7/6/2016"),
                    PriceHistoryId = 2001,
                    Event = "Price Changed",
                    Price = 285_000,
                    Source = "SMARTMLS" }
            };
        }
    }
}
