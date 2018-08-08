using System;
using System.Threading.Tasks;
using Demo.App.Commands;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Extensions;
using NetFusion.Messaging;

namespace Demo.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SampleRpcController : Controller
    {
        private readonly IMessagingService _messaging;
        
        public SampleRpcController(
            IMessagingService messaging)
        {
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
        }
        
        [HttpPost("taxes/property")]
        public async Task CalculatePropertyTax([FromBody]CalculatePropertyTax command)
        {
            var result = await _messaging.SendAsync(command);
            Console.WriteLine(result.ToIndentedJson());
            
        }
    }
}