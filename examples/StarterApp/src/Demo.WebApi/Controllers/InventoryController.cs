using System.Threading.Tasks;
using Demo.Domain.Entities;
using Demo.Domain.Queries;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class InventoryController : Controller
    {
        private readonly IMessagingService _messagingSrv;

        public InventoryController(
            IMessagingService messagingSrv)
        {
            _messagingSrv = messagingSrv;
        }

        [HttpGet("auto/sales/{make}/{year}")]
        public Task<Car[]> GetAutoSales(string make, int year)
        {
            var query = new QueryCarsOnSale(make, year);
            return _messagingSrv.DispatchAsync(query);
        }
    }
}
