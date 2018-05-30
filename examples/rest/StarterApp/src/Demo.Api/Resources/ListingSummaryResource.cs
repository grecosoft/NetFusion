using System;
using NetFusion.Rest.Resources.Hal;

namespace Demo.Api.Resources
{
    public class ListingSummaryResource : HalResource
    {
        public int ListingId { get; set; }
        public string ZipCode { get; set; }
        public DateTime DateListed { get; set; }
        public decimal ListPrice { get; set; }
    }
}