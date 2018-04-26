using Microsoft.Extensions.Logging;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Client.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Client used to make HTTP requests.  
    /// </summary>
    public class RequestClient : IRequestClient
    {
        private HttpClient _httpClient;
        private ILogger _logger;
        private string _correlationHeaderName;
        private IDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;
        private IRequestSettings _defaultRequestSettings;

        private IServiceEntryApiProvider _serviceApiProvider;
        private Action<IRequestSettings> _eachRequestAction;

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

            _mediaTypeSerializers = contentSerializers ?? throw new ArgumentNullException(nameof(contentSerializers),
                "HTTP Context Serializers cannot be null.");

            _defaultRequestSettings = requestSettings ?? throw new ArgumentNullException(nameof(requestSettings),
                "Default Request Settings cannot be null.");
        }

        public Task<HalEntryPointResource> GetApiEntry()
        {
            return _serviceApiProvider.GetEntryPointResource();
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

        // -----

		public Task<ApiResponse> SendAsync(ApiRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (request == null) throw new ArgumentNullException(nameof(request),
                "Request cannot be null.");

            return SendRequest(request, cancellationToken);
        }

		public Task<ApiResponse<TContent>> SendAsync<TContent>(ApiRequest request,
            CancellationToken cancellationToken = default(CancellationToken))
            where TContent : class
        {
            if (request == null) throw new ArgumentNullException(nameof(request),
                "Request cannot be null.");

            return SendRequest<TContent>(request, cancellationToken);
        }

        private async Task<ApiResponse> SendRequest(ApiRequest request, CancellationToken cancellationToken)
        {
            HttpRequestMessage requestMsg = await CreateRequestMessage(request);
            
            _logger.LogDebug("Sending request to: {uri} (method)", requestMsg.RequestUri, requestMsg.Method);
            _logger.LogTrace("Request Message: {request}", JsonConvert.SerializeObject(request, Formatting.Indented));

            HttpResponseMessage responseMsg = await _httpClient.SendAsync(requestMsg, cancellationToken);
            var response = new ApiResponse(requestMsg, responseMsg);

            _logger.LogTrace("Response Message: {response}", JsonConvert.SerializeObject(response, Formatting.Indented));
            return response;
        }

        private async Task<ApiResponse<TContent>> SendRequest<TContent>(ApiRequest request,  CancellationToken cancellationToken)
            where TContent : class
        {
            HttpRequestMessage requestMsg = await CreateRequestMessage(request);
            HttpResponseMessage responseMsg = await _httpClient.SendAsync(requestMsg, cancellationToken);

            TContent resource = null;
            if (responseMsg.IsSuccessStatusCode && responseMsg.Content != null)
            {
                resource = DeserializeResource<TContent>(responseMsg, await responseMsg.Content.ReadAsStreamAsync());
            }

            return new ApiResponse<TContent>(requestMsg, responseMsg, resource);
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

        private T DeserializeResource<T>(HttpResponseMessage responseMsg, Stream responseStream)
            where T : class
        {
            string mediaType = responseMsg.Content.Headers.ContentType.MediaType;
            IMediaTypeSerializer serializer = GetMediaTypeSerializer(mediaType);

            return serializer.Deserialize<T>(responseStream);
        }
    }
}