using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.WebApi.Resources;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Hal;

namespace Demo.WebApi.Controllers
{
    [ApiController]
    [Route("api/listing/price-history")]
    public class PriceHistoryController : ControllerBase
    {
        [HttpGet("{id}")]
        public Task<PriceHistoryModel> GetPriceHistory(int id)
        {
            var history = GetPricingHistory().FirstOrDefault(h => h.PriceHistoryId == id);
            return Task.FromResult(history);
        }

        [HttpGet("{listingId}/events")]
        public Task<IActionResult> GetPriceHistoryEvents(int listingId)
        {
            var historyResources = GetPricingHistory().Where(h => h.ListingId == listingId)
                .Select(h => h.AsResource());
            
            var resource = HalResource.New(r => r.EmbedResources(historyResources, "price-history"));
             
            return Task.FromResult<IActionResult>(Ok(resource));
        }

        private static PriceHistoryModel[] GetPricingHistory()
        {
            return new[] {
                new PriceHistoryModel {
                    ListingId = 1000,
                    DateOfEvent = DateTime.Parse("5/5/2016"),
                    PriceHistoryId = 2000,
                    Event = "Listed",
                    Price = 300_000,
                    Source = "SMARTMLS" },

                new PriceHistoryModel {
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
