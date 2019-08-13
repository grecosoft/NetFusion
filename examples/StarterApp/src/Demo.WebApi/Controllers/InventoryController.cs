using System.Threading.Tasks;
using Demo.Domain.Entities;
using Demo.Domain.Queries;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
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
