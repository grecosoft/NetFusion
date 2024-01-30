using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Web.UnitTests.Mocks;

namespace NetFusion.Web.UnitTests.Rest.ClientRequests.Server;

[ApiController, Route("api/error")]
public class ErrorController(IMockedService mockedService) : ControllerBase
{
    private readonly IMockedService _mockedService = mockedService;

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