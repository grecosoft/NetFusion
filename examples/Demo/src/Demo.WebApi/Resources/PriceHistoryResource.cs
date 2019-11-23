using NetFusion.Rest.Resources.Hal;
using System;

namespace Demo.WebApi.Resources
{
    public class PriceHistoryResource : HalResource
    {
        public int PriceHistoryId { get; set; }
        public int ListingId { get; set; }
        public DateTime DateOfEvent { get; set; }
        public string Event { get; set; }
        public decimal Price { get; set; }
        public string Source { get; set; }
    }
}
