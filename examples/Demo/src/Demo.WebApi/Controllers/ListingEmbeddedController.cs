using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.WebApi.Resources;
using NetFusion.Rest.Resources.Hal;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ListingEmbeddedController : Controller
    {
        [HttpGet("{id}")]
        public Task<IActionResult> GetListing(int id)
        {
            var listingModel = new ListingModel
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

            var listingResource = listingModel.AsResource();
            if (id == 1000)
            {
                var pricingResources = GetPricingHistory().Select(m => m.AsResource()).ToArray();
                listingResource.EmbedResources(pricingResources, "price-history");
            }
            else
            {
                var pricingResources = GetPricingHistory().ToArray();
                listingResource.EmbedModels(pricingResources, "price-history");
            }

            return Task.FromResult<IActionResult>(Ok(listingResource));
        }

        private static IEnumerable<PriceHistoryModel> GetPricingHistory()
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
