using InfrastructureTests.Web.Rest.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InfrastructureTests.Web.Rest.ClientRequests.Server
{
    [Route("api/error")]
    public class ErrorController : Controller
    {
        private IMockedService _mockedService;

        public ErrorController(IMockedService mockedService)
        {
            _mockedService = mockedService;
        }

        [HttpGet("server/side")]
        public void ServerSideError()
        {
            if (_mockedService.TriggerServerSideException)
            {
                throw new InvalidOperationException("Test-Exception");
            }
        }

        [HttpGet("server/http/error-code")]
        public IActionResult HttpErrorCode()
        {
            return StatusCode(StatusCodes.Status404NotFound);   
        }
    }
}
