using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Web.Rest.Client.Settings;
using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.Rest.Client.Core;

/// <summary>
/// Client used to make HTTP requests by delegating to an inner HttpClient.  
/// </summary>
public class RestClient : IRestClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly IDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;

    /// <summary>
    /// Initializes an instance of the client with its associated HttpClient delegated to 
    /// for making network requests.
    /// </summary>
    /// <param name="logger">The logger to use for log messages.</param>
    /// <param name="httpClient">Reference to a HttpClient instance.</param>
    /// <param name="contentSerializers">Dictionary of serializers keyed by media-type.</param>
    public RestClient(ILogger logger,
        HttpClient httpClient,
        IDictionary<string, IMediaTypeSerializer> contentSerializers)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            
        _mediaTypeSerializers = contentSerializers ?? throw new ArgumentNullException(nameof(contentSerializers));
    }
        
    // Sends a request but does not attempt to deserialize the body of the response.
    public async Task<ApiResponse> SendAsync(ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        return await SendRequestBuildResponse(request, cancellationToken, 
            (httpResp, _) => new ApiResponse(httpResp));
    }

    // Sends the request and deserializes the body content into specific type.
    public async Task<ApiResponse<TContent>> SendForContentAsync<TContent>(ApiRequest request,
        CancellationToken cancellationToken = default)
        where TContent : class
    {
        ApiResponse apiResp = await SendRequestBuildResponse(request, typeof(TContent), 
            cancellationToken, 
            (httpResp, contentObj) => new ApiResponse<TContent>(httpResp, (TContent)contentObj));

        return apiResp as ApiResponse<TContent>;
    }

    // Sends a request and deserializes the response body into a HalResource containing
    // the model.  The HalResource adds links and embedded items to the model.  
    public async Task<ApiHalResponse<TModel>> SendForHalAsync<TModel>(ApiRequest request,
        CancellationToken cancellationToken = default)
        where TModel : class
    {
        // Define the type into which body is to be deserialized.
        Type halRespType = typeof(HalResource<>).MakeGenericType(typeof(TModel));
            
        ApiResponse apiResp = await SendRequestBuildResponse(request, halRespType,
            cancellationToken,
            (httpResp, contentObj) =>
            {
                var resource = (HalResource<TModel>)contentObj;
                return new ApiHalResponse<TModel>(httpResp, resource);
            });

        return apiResp as ApiHalResponse<TModel>;
    }
        
    // Sends a request and deserialize the response body into the provided type
    // and sets the State property of the ApiResponse which is defined as Object
    // and not of a specific type.
    public Task<ApiResponse> SendAsync(ApiRequest request, Type contentType, 
        CancellationToken cancellationToken = default)
    {
        return SendRequestBuildResponse(request, contentType, 
            cancellationToken, 
            (httpResp, contentObj) => new ApiResponse(httpResp, contentObj));
    }
        
    private Task<ApiResponse> SendRequestBuildResponse(ApiRequest request,
        CancellationToken cancellationToken,
        Func<HttpResponseMessage, object, ApiResponse> httpResponse)
    {
        return SendRequestBuildResponse(request, null, cancellationToken, httpResponse);
    }
        
    // All other methods delegate to this method that coordinates the common code for
    // sending the request and building the corresponding response.
    private async Task<ApiResponse> SendRequestBuildResponse(ApiRequest request, 
        Type contentType,
        CancellationToken cancellationToken, 
        Func<HttpResponseMessage, object, ApiResponse> httpResponse)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request must be specified.");
        }
            
        HttpRequestMessage requestMsg = await CreateRequestMessage(request);

        LogRequest(request);
        HttpResponseMessage responseMsg = await _httpClient.SendAsync(requestMsg, cancellationToken);

        object contentObj = null;
        if (responseMsg.IsSuccessStatusCode && responseMsg.Content != null && contentType != null)
        {
            await using var stream = await responseMsg.Content.ReadAsStreamAsync();
            contentObj = await DeserializeResource(responseMsg, stream, contentType);
        }

        // Invoke factory method provided by caller so they can build the correct type of response.
        var response = httpResponse(responseMsg, contentObj);

        await SetErrorContext(responseMsg, response);
        LogResponse(response);
            
        return response;
    }

    private void LogRequest(ApiRequest request)
    {
        _logger.LogDebug("Sending request to: {uri} for method: {method}.", request.RequestUri, request.Method);
    }

    private static async Task SetErrorContext(HttpResponseMessage responseMsg, ApiResponse response)
    {
        if (! responseMsg.IsSuccessStatusCode && responseMsg.Content != null)
        {
            var errorBody = await responseMsg.Content.ReadAsStringAsync();
            response.SetErrorContent(errorBody);
        }
    }

    private void LogResponse(ApiResponse response)
    {
        var request = response.Request;
            
        _logger.LogDebug(
            "Response received for: {uri} with method: {method}. " + 
            "Received status code: {statusCode} with reason: {reasonPhase}.",
            request.RequestUri, 
            request.Method, 
            response.StatusCode, 
            response.ReasonPhase);
    }
        
    private async Task<HttpRequestMessage> CreateRequestMessage(ApiRequest request)
    {
        AssertRequest(request);
        SetEmbeddedQueryString(request);
            
        string requestUri = await BuildRequestUrl(request);
        var requestMsg = new HttpRequestMessage(request.Method, requestUri);
            
        request.Settings.Apply(requestMsg);
        SerializeRequestContent(request, requestMsg);
            
        return requestMsg;
    }

    private static void AssertRequest(ApiRequest request)
    {
        if (request.IsTemplate)
        {
            throw new InvalidOperationException(
                $"Request URI: {request.RequestUri}.  Templates must have all route-tokens populated " + 
                "before making request to server.");
        }
    }

    // Check if the request has any embedded names specified.  This allows the client
    // to suggest to the server the subset of resources it needs from the default set
    // of embedded resources normally returned from the server API.
    private static void SetEmbeddedQueryString(ApiRequest request)
    {
        if (! string.IsNullOrWhiteSpace(request.EmbeddedNames))
        {
            request.Settings.QueryString.AddParam("embed", request.EmbeddedNames);
        }
    }

    private static async Task<string> BuildRequestUrl(ApiRequest request)
    {
        var requestSettings = request.Settings;
            
        // Build the query string and append to request URL.
        if (requestSettings.QueryString.Params.Any())
        {
            var encodedContent = new FormUrlEncodedContent(requestSettings.QueryString.Params);
            return $"{request.RequestUri}?{await encodedContent.ReadAsStringAsync()}";
        }

        return request.RequestUri;
    }

    private void SerializeRequestContent(ApiRequest request, HttpRequestMessage requestMsg)
    {
        var requestSettings = request.Settings;
            
        // Serialize the body of the request based on its content-type.
        if (request.Content != null)
        {
            if (requestSettings.Headers.ContentType == null)
            {
                throw new InvalidOperationException(
                    $"The request for: {request.RequestUri} contains content without having the Content-Type header specified.");
            }

            requestMsg.Content = CreateContentForMediaType(requestSettings.Headers.ContentType, request.Content);
        }
    }
        
    private HttpContent CreateContentForMediaType(HeaderValue headerValue, object content)
    {
        string mediaType = headerValue.Value.First();
        IMediaTypeSerializer serializer = GetMediaTypeSerializer(mediaType);

        var requestContent = new ByteArrayContent(serializer.Serialize(content));
        requestContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

        return requestContent;
    }

    private IMediaTypeSerializer GetMediaTypeSerializer(string mediaType)
    {
        if (! _mediaTypeSerializers.TryGetValue(mediaType, out IMediaTypeSerializer contentSerializer))
        {
            throw new InvalidOperationException(
                $"Content Serializer not registered for media type: {mediaType}.");
        }

        return contentSerializer;
    }

    private async Task<object> DeserializeResource(HttpResponseMessage responseMsg, 
        Stream responseStream,
        Type modelType)
    {
        string mediaType = responseMsg.Content.Headers.ContentType.MediaType;
        IMediaTypeSerializer serializer = GetMediaTypeSerializer(mediaType);

        return await serializer.Deserialize(responseStream, modelType);
    }
}