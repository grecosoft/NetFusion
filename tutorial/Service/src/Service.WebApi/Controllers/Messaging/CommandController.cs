using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Messaging;
using Service.Domain.Commands;
using Service.WebApi.Models;

namespace Service.WebApi.Controllers.Messaging
{
    [ApiController, Route("api/messaging/commands")]
    public class CommandController : ControllerBase
    {
        private readonly IMessagingService _messaging;
        
        public CommandController(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        [HttpPost("range")]
        public async Task<IActionResult> GetRanges([FromBody]RangeModel model)
        {
            var firstRange = new CalculateRange(model.Template, model.SetOne);
            var secondRange = new CalculateRange(model.Template, model.SetTwo);

            var rangeOneResult = _messaging.SendAsync(firstRange);
            var rangeTwoResult = _messaging.SendAsync(secondRange);

            await Task.WhenAll(rangeOneResult, rangeTwoResult);

            return Ok(new []
            {
                rangeOneResult.Result.Message,
                rangeTwoResult.Result.Message
            });
        }
    }
}