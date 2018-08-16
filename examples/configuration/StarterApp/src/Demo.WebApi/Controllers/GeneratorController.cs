using System.Collections.Generic;
using System.Linq;
using Demo.Infra;
using Microsoft.AspNetCore.Mvc;

namespace Demo.WebApi.Controllers
{
    [Route("api/generators")]
    public class GeneratorController : Controller
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
