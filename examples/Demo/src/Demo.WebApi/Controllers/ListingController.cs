using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Demo.WebApi.Resources;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Web.Mvc.Metadata;

namespace Demo.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]"), GroupMeta("Listing")]
    public class ListingController : ControllerBase
    {
        [HttpGet("entry")]
        public HalEntryPointResource GetEntryPoint()
        {
            var model = new EntryPointModel
            {
                Version = GetType().Assembly.GetName().Version.ToString()
            };
            
            return new HalEntryPointResource(model);
        }
        
        [HttpGet("{id}"), ActionMeta("GetListing")]
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
                YearBuild = 1986
            };

            var resource = listingModel.AsResource();
            return Task.FromResult<IActionResult>(Ok(resource));

        }

        [HttpPut("{id}")]
        public Task<ListingModel> UpdateListing(int id, ListingModel listing)
        {
            listing.ListingId = id;
            return Task.FromResult(listing);
        }

        [HttpDelete("{id}")]
        public string DeleteListing(int id)
        {
            return $"DELETED: {id}";
        }
    }
}
