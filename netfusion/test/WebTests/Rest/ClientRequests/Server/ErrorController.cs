using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebTests.Rest.Setup;

namespace WebTests.Rest.ClientRequests.Server
{
    [Route("api/error")]
    public class ErrorController : Controller
    {
        private readonly IMockedService _mockedService;

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
