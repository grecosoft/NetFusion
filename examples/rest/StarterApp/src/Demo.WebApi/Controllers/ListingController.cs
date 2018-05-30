using Demo.Api.Resources;
using Demo.WebApi.Resources;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Server.Resources;
using NetFusion.Web.Mvc.Metadata;
using System;
using System.Threading.Tasks;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]"),
        GroupMeta("Listing")]
    public class ListingController : Controller
    {
        [HttpGet("entry")]
        public EntryPointResource GetEntryPoint()
        {
            return new EntryPointResource();
        }

        [HttpGet("{id}"),
             ActionMeta("GetListing")]
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

            return Task.FromResult(listing);
        }

        [HttpPut("{id}"), 
            ActionMeta("GetListing")]
        public Task<ListingResource> UpdateListing(int id, ListingResource listing)
        {
            listing.ListingId = id;
            return Task.FromResult(listing);
        }

        [HttpDelete("{id}"), ResourceType(typeof(ListingResource))]
        public string DeleteListing(int id)
        {
            return $"DELETED: {id}";
        }

        [HttpGet("summary/{id}"), 
            ActionMeta("GetListingSummary")]
        public Task<ListingSummaryResource> GetListingSummary(int id)
        {
            var listing = new ListingSummaryResource
            {
                ListingId = id,
                ZipCode = "06410",
                DateListed = DateTime.Parse("5/5/2016"),
                ListPrice = 300_000M
            };

            return Task.FromResult(listing);
        }
    }
}
