using Demo.Api.Models;
using NetFusion.Messaging.Types;

namespace Demo.Api.Queries
{
    public class QueryCarsOnSale : Query<Car[]>
    {
        public string Make { get; }
        public int Year { get; }

        public QueryCarsOnSale(
            string make,
            int year)
        {
            Make = make;
            Year = year;
        }
    }
}