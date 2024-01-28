using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Web.Rest.Client;

namespace NetFusion.Web.UnitTests.Hosting;

/// <summary>
/// Provides methods for asserting the request made to the TestServer.
/// </summary>
public class WebServerAssert
{
    private readonly IServiceProvider _services;
    private readonly HttpResponseMessage _httpResponse;
    private readonly ApiResponse _apiResponse;
        
    public WebServerAssert(IServiceProvider services,
        HttpResponseMessage httpResponse,
        ApiResponse apiResponse)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _httpResponse = httpResponse;
        _apiResponse = apiResponse;
    }

    /// <summary>
    /// Allows the response issued with the HTTPClient to be asserted.
    /// </summary>
    /// <param name="assert">Delegate passed the response to be asserted.</param>
    /// <returns>Self Reference</returns>
    public WebServerAssert HttpResponse(Action<HttpResponseMessage> assert)
    {
        ArgumentNullException.ThrowIfNull(assert);

        if (_httpResponse == null)
        {
            throw new InvalidOperationException("HttpClient was not acted on.");
        }

        assert(_httpResponse);
        return this;
    }

    public async Task<WebServerAssert> HttpResponseAsync(Func<HttpResponseMessage, Task> assert)
    {
        ArgumentNullException.ThrowIfNull(assert);

        if (_httpResponse == null)
        {
            throw new InvalidOperationException("HttpClient was not acted on.");
        }

        await assert(_httpResponse);
        return this;
    }

    /// <summary>
    /// Allows the response issued with the IRequestClient to be asserted.
    /// </summary>
    /// <param name="assert">Delegate passed the response to be asserted.</param>
    /// <returns>Self Reference</returns>
    public WebServerAssert ApiResponse(Action<ApiResponse> assert)
    {
        ArgumentNullException.ThrowIfNull(assert);

        if (_apiResponse == null)
        {
            throw new InvalidOperationException("IRequestClient as not acted on.");
        }

        assert(_apiResponse);
        return this;
    }
        
    /// <summary>
    /// Allows a registered service instance to be asserted.
    /// </summary>
    /// <param name="assert">Delegate passed the service instance to be asserted.</param>
    /// <typeparam name="TService">The registered service type for the instance to be asserted.</typeparam>
    /// <returns>Self Reference</returns>
    public WebServerAssert Service<TService>(Action<TService> assert)
    {
        ArgumentNullException.ThrowIfNull(assert);

        var instance = _services.GetRequiredService<TService>();
        assert(instance);
            
        return this;
    }
}