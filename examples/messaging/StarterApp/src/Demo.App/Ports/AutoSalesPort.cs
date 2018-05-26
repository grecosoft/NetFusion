using System.Threading.Tasks;
using System.Linq;
using Demo.Api.Models;
using Demo.Api.Queries;
using Demo.Infra;
using NetFusion.Messaging;
using System;

namespace Demo.App.Ports
{
    public class AutoSalesPort : IQueryConsumer
    {
        private ISalesDataAdapter _adapter;

        public AutoSalesPort(ISalesDataAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task<Car[]> Search(QueryCarsOnSale query)
        {
            var results = await _adapter.GetInventory(query.Make, query.Year);

            if (query.Make == "Yugo")
            {
                throw new InvalidOperationException(
                    "Make is not allowed to be sold in US.");
            }
            
            return results.Select(
                d => new Car {
                    Make = d.Make,
                    Model = d.Model,
                    Year = d.Year,
                    Color = d.Color,
                    Price = d.Price
                }).ToArray();
        }
    }
}