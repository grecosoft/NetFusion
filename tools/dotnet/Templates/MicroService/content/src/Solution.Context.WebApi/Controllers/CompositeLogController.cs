using System;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Bootstrap.Container;

namespace Solution.Context.WebApi.Controllers
{
    [ApiController, Route("api/composite")]
    public class CompositeController : ControllerBase
    {
        private readonly ICompositeApp _compositeApp;
        
        public CompositeController(ICompositeApp compositeApp)
        {
            _compositeApp = compositeApp ?? throw new ArgumentNullException(nameof(compositeApp));
        }

        [HttpGet("log")]
        public IActionResult GetLog()
        {
            return Ok(_compositeApp.Log);
        }
        
    }
}