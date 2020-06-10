using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using Service.Domain.Queries;

namespace Service.WebApi.Controllers.Messaging
{
    [ApiController, Route("api/messaging/queries")]
    public class QueryController : ControllerBase
    {
        private readonly IMessagingService _messaging;
        
        public QueryController(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpGet("customers/{customerId}/addresses")]
        public async Task<IActionResult> GetCustomerAddresses(int customerId)
        {
            var query = new QueryCustomerAddresses(customerId);
            var addresses = await _messaging.DispatchAsync(query);

            return Ok(addresses);
        }
    }
}