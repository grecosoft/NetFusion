using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.WebApi.Resources;
using NetFusion.Rest.Resources.Hal;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ListingEmbeddedController : Controller
    {
        [HttpGet("{id}")]
        public Task<ListingResource> GetListing(int id)
        {
            var listing = new ListingResource
            {
                ListingId = id,
                AcresLot = 3,
                Address = "112 Main Avenue",
                City = "Cheshire",
                State = "CT",
                ZipCode = "06410",
                DateListed = DateTime.Parse("5/5/2016"),
                ListPrice = 300_000M,
                NumberBeds = 5,
                NumberFullBaths = 3,
                NumberHalfBaths = 2,
                SquareFeet = 2500,
                YearBuild = 1986,
            };

            var priceHistory = GetPricingHistory();
            listing.Embed(priceHistory, "price-history");

            return Task.FromResult(listing);
        }

        private static IEnumerable<PriceHistoryResource> GetPricingHistory()
        {
            return new[] {
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
