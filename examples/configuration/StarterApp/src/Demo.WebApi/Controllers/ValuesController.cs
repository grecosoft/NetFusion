using Demo.Infra;
using Microsoft.AspNetCore.Mvc;

namespace Demo.WebApi.Controllers
{
    [Route("api/values")]
    public class ValuesController: Controller
    {
        private ICalculatorModule _calculator;
        private ICalculateService _calculateService;

        public ValuesController(
            ICalculatorModule calculator,
            ICalculateService calculateService)
        {
            _calculator = calculator;
            _calculateService = calculateService;
        }

        [HttpPost("calculate")]
        public int Calculate([FromBody]ValueModel data)
        {
            return _calculator.CalculateValue(data.Values);
        }

        [HttpPost("calculate/service")]
        public int CalculateUsingService([FromBody]ValueModel data)
        {
            return _calculateService.CalculateValue(data.Values);
        }

        public class ValueModel
        {
            public int[] Values { get; set; }
        }
    }
}
