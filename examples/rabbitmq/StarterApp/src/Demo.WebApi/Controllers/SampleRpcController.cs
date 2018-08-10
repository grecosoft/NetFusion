using System;
using System.Threading.Tasks;
using Demo.App.Commands;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<TaxCalc> CalculatePropertyTax([FromBody]CalculatePropertyTax command)
        {
            return await _messaging.SendAsync(command);
        }
        
        [HttpPost("taxes/auto")]
        public async Task<TaxCalc> CalculateAutoTax([FromBody]CalculateAutoTax command)
        {
            return await _messaging.SendAsync(command);
        }
    }
}