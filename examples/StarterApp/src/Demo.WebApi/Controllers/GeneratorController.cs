using System.Collections.Generic;
using System.Linq;
using Demo.Core;
using Microsoft.AspNetCore.Mvc;

namespace Demo.WebApi.Controllers
{
    [Route("api/generators")]
    [ApiController]
    public class GeneratorController : ControllerBase
    {
        private IEnumerable<INumberGenerator> _generators;

        public GeneratorController(
            IEnumerable<INumberGenerator> generators)
        {
            _generators = generators;
        }

        [HttpGet("numbers")]
        public int[] GenerateNumbers()
        {
            var numbers =_generators.Select(g => g.GenerateNumber())
                .ToArray();

            return numbers;
        }
    }
}
