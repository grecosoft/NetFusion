using NetFusion.Rest.Resources.Hal;
using System;

namespace Demo.Api.Resources
{
    public class ListingResource : HalResource
    {
        public int ListingId { get; set; }
        public DateTime DateListed { get; set; }
        public int NumberBeds { get; set; }
        public int NumberFullBaths { get; set; }
        public int NumberHalfBaths { get; set; }
        public int SquareFeet { get; set; }
        public decimal AcresLot { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public decimal ListPrice { get; set; }
        public int YearBuild { get; set; }
    }
}
