using System;
using System.Net.Http;
using NetFusion.Web.Rest.Client;

namespace NetFusion.Web.UnitTests.Hosting;

/// <summary>
/// The response returned by sending an HttpRequest to the TestServer that can be asserted.
/// </summary>
public class WebServerResponse
{
    private readonly IServiceProvider _services;
        
    private readonly HttpResponseMessage _httpResponse;
    private readonly ApiResponse _apiResponse;
        
        
    public WebServerResponse(IServiceProvider services, HttpResponseMessage response)
    {
        _services = services;
        _httpResponse = response ?? throw new ArgumentNullException(nameof(response));
    }
        
    public WebServerResponse(IServiceProvider services, ApiResponse response)
    {
        _services = services;
        _apiResponse = response ?? throw new ArgumentNullException(nameof(response));
    }
        
    /// <summary>
    /// Allows asserting the returned server response.
    /// </summary>
    public WebServerAssert Assert => new WebServerAssert(_services, _httpResponse, _apiResponse);
}