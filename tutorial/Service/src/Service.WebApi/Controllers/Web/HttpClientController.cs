using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Rest.Client;

namespace Service.WebApi.Controllers.Web
{
    [ApiController, Route("api/http/client")]
    public class HttpClientController : ControllerBase
    {
        private static IRestClientFactory _requestClientFactory;

        public HttpClientController(IRestClientFactory requestClientFactory)
        {
            _requestClientFactory = requestClientFactory;
        }
        
        [HttpGet("test")]
        public async Task<IActionResult> TestCall()
        {
            var requestClient = _requestClientFactory.CreateClient("test");

            var apiRequest = ApiRequest.Get("api/sensors/s87hss87d");
            var apiResponse = await requestClient.SendAsync<SensorModel>(apiRequest);

            var sensor = apiResponse.Resource;

            return Ok(sensor);
        }
        
    }
    
    public class SensorModel
    {
        public string SensorId { get; set; }
        public string SensorName { get; set; }
        public bool IsActive { get; set; }
        public string Version { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
    }
}