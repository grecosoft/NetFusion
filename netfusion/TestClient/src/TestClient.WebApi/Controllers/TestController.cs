namespace TestClient.WebApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TestClient.Domain.Services;

    [Route("api/test")]
    public class TestController : Controller
    {
        private readonly ITestService _testService;
        
        public TestController(ITestService testService)
        {
            _testService = testService;
        }

        [HttpGet]
        public string GetValue()
        {
            return _testService.GetValue();
        }
    }
}