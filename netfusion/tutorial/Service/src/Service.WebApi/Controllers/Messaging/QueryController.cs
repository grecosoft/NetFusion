using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using NetFusion.Web.Mvc.Metadata;

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
    }
}