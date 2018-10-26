using System.Threading.Tasks;
using System.Linq;
using Demo.Domain.Entities;
using Demo.Domain.Queries;
using Demo.Domain.Adapters;
using NetFusion.Messaging;
using System;

namespace Demo.Domain.Handlers
{
    public class AutoSalesHandler : IQueryConsumer
    {
        private ISalesDataAdapter _adapter;

        public AutoSalesHandler(ISalesDataAdapter adapter)
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