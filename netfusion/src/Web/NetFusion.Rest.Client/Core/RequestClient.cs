using Microsoft.Extensions.Logging;
using NetFusion.Rest.Client.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NetFusion.Rest.Resources.Hal;

namespace NetFusion.Rest.Client.Core
{
    using System.Collections.Concurrent;

    /// <summary>
    /// Client used to make HTTP requests.  
    /// </summary>
    public class RequestClient : IRequestClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private string _correlationHeaderName;
        private readonly ConcurrentDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;
        private readonly IRequestSettings _defaultRequestSettings;

        // Optional registered services specified using RequestClientBuilder:
        private IServiceEntryApiProvider _serviceApiProvider;
        private Action<IRequestSettings> _eachRequestAction;
        private IDictionary<HttpStatusCode, Func<ErrorStatusContext, Task<bool>>> _statusHandlers;

        /// <summary>
        /// Initializes an instance of the client with its associated HttpClient delegated to 
        /// for making network requests.
        /// </summary>
        /// <param name="httpClient">Reference to a HttpClient instance.</param>
        /// <param name="logger">The logger to use for log messages.</param>
        /// <param name="contentSerializers">Dictionary of serializers keyed by media-type.</param>
        /// <param name="requestSettings">The default request settings to be used for each request.</param>
        public RequestClient(HttpClient httpClient, 
            ILogger logger,
            IDictionary<string, IMediaTypeSerializer> contentSerializers, 
            IRequestSettings requestSettings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), 
                "HTTP Client cannot be null.");
            
            _logger = logger ?? throw new ArgumentNullException(nameof(httpClient), 
                "Logger cannot be null.");

            _defaultRequestSettings = requestSettings ?? throw new ArgumentNullException(nameof(requestSettings),
                "Default Request Settings cannot be null.");

            if (contentSerializers == null)
            {
                throw new ArgumentNullException(nameof(contentSerializers), "HTTP Context Serializers cannot be null.");            
            }
            
            _mediaTypeSerializers = new ConcurrentDictionary<string, IMediaTypeSerializer>(contentSerializers);
        }

        public async Task<HalEntryPointResource> GetApiEntry()
        {
            return await _serviceApiProvider.GetEntryPointResource().ConfigureAwait(false);
        }

        // Called by builder when creating client...
        // ---------------------------------------------
        internal void SetApiServiceProvider(IServiceEntryApiProvider provider)
        {
            _serviceApiProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        internal void SetEachRequestAction(Action<IRequestSettings> config)
        {
            _eachRequestAction = config ?? throw new ArgumentNullException(nameof(config));
        }

        internal void AddCorrelationId(string headerName)
        {
            if (string.IsNullOrWhiteSpace(headerName))
                throw new ArgumentException("Correlation header name not specified.", nameof(headerName));

            _correlationHeaderName = headerName;
        }

        internal void SetStatusCodeHandlers(IDictionary<HttpStatusCode, Func<ErrorStatusContext, Task<bool>>> handlers)
        {
            _statusHandlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
        }

        // -----

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
            responseMsg = await HandleIfErrorStatusCode(request, responseMsg, cancellationToken);

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
            responseMsg = await HandleIfErrorStatusCode(request, responseMsg, cancellationToken);

            if (responseMsg.IsSuccessStatusCode && responseMsg.Content != null)
            {
                resource = await DeserializeResource<HalResource<TContent>>(responseMsg, await responseMsg.Content.ReadAsStreamAsync());
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
            responseMsg = await HandleIfErrorStatusCode(request, responseMsg, cancellationToken);

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
        
        private async Task<HttpResponseMessage> HandleIfErrorStatusCode(
            ApiRequest request,
            HttpResponseMessage responseMsg,
            CancellationToken cancellationToken)
        {
            if (responseMsg.IsSuccessStatusCode || (request.Settings?.SuppressStatusCodeHandlers ?? false))
            {
                return responseMsg;
            }

            // Determine if there is a registered handler for the status code:
            if (_statusHandlers == null || !_statusHandlers.ContainsKey(responseMsg.StatusCode))
            {
                return responseMsg;
            }
            
            _logger.LogDebug($"Handler for response status code: {responseMsg.StatusCode} being called.");

            var handler = _statusHandlers[responseMsg.StatusCode];
            
            var context = new ErrorStatusContext(this, responseMsg, 
                _defaultRequestSettings, 
                request.Settings);

            // Call the handler associated with the status code so it can determine
            // if the request should be retried:
            bool retryRequest = await handler(context);
            if (retryRequest)
            {
                _logger.LogDebug($"Attempting to retry request: {request.RequestUri}");
                
                HttpRequestMessage requestMsg = await CreateRequestMessage(request);
                responseMsg = await _httpClient.SendAsync(requestMsg, cancellationToken);
            }

            return responseMsg;
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

            var requestSettings = _defaultRequestSettings.GetMerged(request.Settings);
            request.UsingSettings(requestSettings);

            // Check if the request has any embedded names specified.  This allows the client
            // to suggest to the server the subset of resources it needs from the default set
            // of embedded resources normally returned from the server API.
            if (!string.IsNullOrWhiteSpace(request.EmbeddedNames))
            {
                requestSettings.QueryString.AddParam("embed", request.EmbeddedNames);
            }

            // Add correlation value to request if configured.
            if (_correlationHeaderName != null)
            {
                var correlationId = Guid.NewGuid().ToString();
                requestSettings.Headers.Add(_correlationHeaderName, correlationId);
            }

            _eachRequestAction?.Invoke(requestSettings);

            // Build the query string and append to request URL.
            if (requestSettings.QueryString.Params.Any())
            {
                var encodedContent = new FormUrlEncodedContent(requestSettings.QueryString.Params);
                requestUri += "?" + await encodedContent.ReadAsStringAsync();
            }

            var requestMsg = new HttpRequestMessage(request.Method, requestUri);
            requestSettings.Apply(requestMsg);

			if (request.Content != null)
			{
                if (requestSettings.Headers.ContentType == null)
                {
                    throw new InvalidOperationException(
                        $"The request for:  {requestUri} contains content without having the Content-Type header specified.");
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

            if (!_mediaTypeSerializers.TryGetValue(mediaType, out IMediaTypeSerializer contentSerializer))
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