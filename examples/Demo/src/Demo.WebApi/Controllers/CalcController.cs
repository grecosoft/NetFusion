using Demo.Core;
using Microsoft.AspNetCore.Mvc;

namespace Demo.WebApi.Controllers
{
    [Route("api/calc")]
    [ApiController]
    public class CalcController : ControllerBase
    {
        private readonly ICalculateService _calculateService;

        public CalcController(
            ICalculateService calculateService)
        {
            _calculateService = calculateService;
        }

        [HttpPost("values")]
        public int Calculate([FromBody]ValueModel data)
        {
            return _calculateService.CalculateValue(data.Values);
        }

        public class ValueModel
        {
            public int[] Values { get; set; }
        }
    }
}
