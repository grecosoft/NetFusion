using NetFusion.Rest.Client.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Client used to make HTTP requests.  
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
        

		public async Task<ApiResponse> SendAsync(ApiRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request),
                "Request cannot be null.");

            return await SendRequest(request, cancellationToken).ConfigureAwait(false);
        }

		public async Task<ApiResponse<TContent>> SendAsync<TContent>(ApiRequest request,
            CancellationToken cancellationToken = default)
            where TContent : class
        {
            if (request == null) throw new ArgumentNullException(nameof(request),
                "Request cannot be null.");

            return await SendRequest<TContent>(request, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task<ApiResponse> SendAsync(ApiRequest request, Type contentType, 
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));

            return await SendRequest(request, contentType, cancellationToken).ConfigureAwait(false);
        }

        private async Task<ApiResponse> SendRequest(ApiRequest request, CancellationToken cancellationToken)
        {
            HttpRequestMessage requestMsg = await CreateRequestMessage(request);
            LogRequest(request);
            
            HttpResponseMessage responseMsg = await _httpClient.SendAsync(requestMsg, cancellationToken);

            var response = new ApiResponse(requestMsg, responseMsg);
            await SetErrorContext(responseMsg, response);

            LogResponse(response);
            return response;
        }

        private async Task<ApiResponse<TContent>> SendRequest<TContent>(ApiRequest request,  CancellationToken cancellationToken)
            where TContent : class
        {
            HttpRequestMessage requestMsg = await CreateRequestMessage(request);
            LogRequest(request);
            
            HalResource<TContent> resource = null;
            HttpResponseMessage responseMsg = await _httpClient.SendAsync(requestMsg, cancellationToken);

            if (responseMsg.IsSuccessStatusCode && responseMsg.Content != null)
            {
                resource = await DeserializeResource<HalResource<TContent>>(responseMsg, 
                    await responseMsg.Content.ReadAsStreamAsync());
            }

            var response = new ApiResponse<TContent>(requestMsg, responseMsg, resource);
            await SetErrorContext(responseMsg, response);
            
            LogResponse(response);
            return response;
        }
        
        private async Task<ApiResponse> SendRequest(ApiRequest request, Type contentType, 
            CancellationToken cancellationToken)
        {
            HttpRequestMessage requestMsg = await CreateRequestMessage(request);

            LogRequest(request);
            HttpResponseMessage responseMsg = await _httpClient.SendAsync(requestMsg, cancellationToken);

            object resource = null;
            if (responseMsg.IsSuccessStatusCode && responseMsg.Content != null)
            {
                resource = DeserializeResource(responseMsg, 
                    await responseMsg.Content.ReadAsStreamAsync(), 
                    contentType);
            }

            var response = new ApiResponse(requestMsg, responseMsg, resource);
            await SetErrorContext(responseMsg, response);
            
            LogResponse(response);
            return response;
        }

        private void LogRequest(ApiRequest request)
        {
            _logger.LogTrace("Sending request to: {uri} for method: {method}.", request.RequestUri, request.Method);
        }

        private static async Task SetErrorContext(HttpResponseMessage responseMsg, ApiResponse response)
        {
            if (! responseMsg.IsSuccessStatusCode && responseMsg.Content != null)
            {
                var errorBody = await responseMsg.Content.ReadAsStringAsync();
                response.SetErrorContext(errorBody);
            }
        }

        private void LogResponse(ApiResponse response)
        {
            var request = response.Request;
            
            _logger.LogTrace(
                "Response received for: {uri} with method: {method}. " + 
                "Received status code: {statusCode} with reason: {reasonPhase}.",
                request.RequestUri, 
                request.Method, 
                response.StatusCode, 
                response.ReasonPhase);
        }
        
        private async Task<HttpRequestMessage> CreateRequestMessage(ApiRequest request)
        {
            string requestUri = request.RequestUri;

            if (request.IsTemplate)
            {
                throw new InvalidOperationException(
                    $"Request URI: {request.RequestUri}.  Templates must have all route-tokens populated " + 
                    $"before making request to server.");
            }
            
            var requestSettings = request.Settings;

            // Check if the request has any embedded names specified.  This allows the client
            // to suggest to the server the subset of resources it needs from the default set
            // of embedded resources normally returned from the server API.
            if (! string.IsNullOrWhiteSpace(request.EmbeddedNames))
            {
                requestSettings.QueryString.AddParam("embed", request.EmbeddedNames);
            }

            // Build the query string and append to request URL.
            if (requestSettings.QueryString.Params.Any())
            {
                var encodedContent = new FormUrlEncodedContent(requestSettings.QueryString.Params);
                requestUri += "?" + await encodedContent.ReadAsStringAsync();
            }

            var requestMsg = new HttpRequestMessage(request.Method, requestUri);
            requestSettings.Apply(requestMsg);

            // Serialize the body of the request based on its content-type.
			if (request.Content != null)
			{
                if (requestSettings.Headers.ContentType == null)
                {
                    throw new InvalidOperationException(
                        $"The request for: {requestUri} contains content without having the Content-Type header specified.");
                }

				requestMsg.Content = CreateContentForMediaType(requestSettings.Headers.ContentType, request.Content);
			}
            return requestMsg;
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

        private Task<T> DeserializeResource<T>(HttpResponseMessage responseMsg, Stream responseStream)
            where T : class
        {
            string mediaType = responseMsg.Content.Headers.ContentType.MediaType;
            IMediaTypeSerializer serializer = GetMediaTypeSerializer(mediaType);

            return serializer.Deserialize<T>(responseStream);
        }
        
        private object DeserializeResource(HttpResponseMessage responseMsg, 
            Stream responseStream,
            Type resourceType)
        {
            string mediaType = responseMsg.Content.Headers.ContentType.MediaType;
            IMediaTypeSerializer serializer = GetMediaTypeSerializer(mediaType);

            return serializer.Deserialize(responseStream, resourceType);
        }
    }
}