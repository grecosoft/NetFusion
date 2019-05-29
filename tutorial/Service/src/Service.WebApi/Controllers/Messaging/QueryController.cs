using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Web.Mvc.Metadata;
using Service.Domain.Queries;

namespace Service.WebApi.Controllers.Messaging
{
    [Route("api/messaging/queries"),
     GroupMeta(nameof(CommandController))]
    public class QueryController : Controller
    {
        private readonly IMessagingService _messaging;
        
        public QueryController(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpGet("customers/{customerId}/addresses"), ActionMeta(nameof(GetCustomerAddresses))]
        public async Task<IActionResult> GetCustomerAddresses(int customerId)
        {
            var query = new QueryCustomerAddresses(customerId);
            var addresses = await _messaging.DispatchAsync(query);

            return Ok(addresses);
        }
    }
}