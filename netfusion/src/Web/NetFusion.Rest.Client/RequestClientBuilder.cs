using Microsoft.Extensions.Logging;
using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Class used to build an instance of a IRequestClient based on a set
    /// of configurations.
    /// </summary>
    public class RequestClientBuilder : IServiceEntryApiProvider
    {
        public const string CorrelationHeaderName = "nf_CorrelationId";
        private readonly string _baseAddressUri;
        private string _entryPointPath;
        private readonly ClientSettings _clientSettings;

        private IRequestSettings _defaultRequestSettings;
        private Action<IRequestSettings> _eachRequestAction;
        private IDictionary<HttpStatusCode, Func<ErrorStatusContext, Task<bool>>> _statusCodeHandlers;
        private IDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;

        private string _correlationIdHeaderName = "NF_CorrelationId";
        private Lazy<Task<HalEntryPointResource>> _apiEntryPointLazy;
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;

        private RequestClientBuilder(string baseAddressUri, ClientSettings clientSettings)
        {
            if (string.IsNullOrWhiteSpace(baseAddressUri))
                throw new ArgumentException("Base address must be specified.", nameof(baseAddressUri));

            _clientSettings = clientSettings ?? new ClientSettings();
            _baseAddressUri = baseAddressUri;

            InitServicePointManager(baseAddressUri, _clientSettings);
        }

        /// <summary>
        /// Creates a new builder for a specific base service Uri.
        /// </summary>
        /// <param name="baseAddressUrl">The base address to which the client will send requests.</param>
        /// <param name="settings">Settings used to configure underlying HttpClient.</param>
        /// <returns>Instance of the builder class.</returns>
        public static RequestClientBuilder ForBaseAddress(string baseAddressUrl, ClientSettings settings = null)
        {
            return new RequestClientBuilder(baseAddressUrl, settings);
        }

        /// <summary>
        /// Sets a child path of the base address that is to be invoked to load the 
        /// root service API methods to which the client can communicated.  All other
        /// URLs are obtained from returned resources.
        /// </summary>
        /// <param name="entryPointPath">Child path of bass address (Optional).</param>
        /// <returns>Builder for method chaining.</returns>
        public RequestClientBuilder ForEntryPoint(string entryPointPath)
        {
            if (string.IsNullOrWhiteSpace(entryPointPath))
                throw new ArgumentException("Service Api Entry point must be specified", nameof(entryPointPath));
            
            _entryPointPath = entryPointPath;
            return this;
        }

        /// <summary>
        /// Specifies the default settings that should be used for each request.  Any request specific settings,
        /// if specified, will be merged into the default settings.  If not specified, the default settings are
        /// initialized for the most common usage.
        /// </summary>
        /// <param name="requestSettings">Request setting to use for each request.</param>
        /// <returns>Builder for method chaining.</returns>
        public RequestClientBuilder UsingDefaultSettings(IRequestSettings requestSettings)
        {
            _defaultRequestSettings = requestSettings ?? 
                throw new ArgumentNullException(nameof(requestSettings));
            return this;
        }

        /// <summary>
        /// Specifies the logger factory to use.
        /// </summary>
        /// <param name="factory">The logger factory to use to log request/response messages.</param>
        /// <returns>Builder for method chaining.</returns>
        public RequestClientBuilder UsingLoggerFactory(ILoggerFactory factory)
        {
            _loggerFactory = factory ?? 
                throw new ArgumentNullException(nameof(factory));
            return this;
        }

        /// <summary>
        /// Indicates that a header should be added containing a correlation id value.
        /// </summary>
        /// <param name="headerName">The name of the header to use.  If not specified, 
        /// a default value is used.</param>
        /// <returns>Builder for method chaining.</returns>
        public RequestClientBuilder AddRequestCorrelationId(string headerName = CorrelationHeaderName)
        {
            if (string.IsNullOrWhiteSpace(headerName))
            {
                throw new ArgumentException("Header name not specified.", nameof(headerName));
            }

            _correlationIdHeaderName = headerName;
            return this;
        }

        /// <summary>
        /// Registers a delegate that will be invoked before each request.
        /// </summary>
        /// <param name="settings">Reference to the request settings being used for the current request.</param>
        /// <returns>Builder for method chaining.</returns>
        public RequestClientBuilder BeforeEachRequest(Action<IRequestSettings> settings)
        {
            _eachRequestAction = settings ?? throw new ArgumentNullException(nameof(settings));
            return this;
        }

        /// <summary>
        /// Registers a handler to be called when a specific HTTP status code is returned.
        /// </summary>
        /// <param name="code">The status code to register handler.</param>
        /// <param name="handler">The handler to call for a HTTP status code.</param>
        /// <returns>Builder for method chaining.</returns>
        public RequestClientBuilder OnStatusCode(HttpStatusCode code, Func<ErrorStatusContext, Task<bool>> handler)
        {
            _statusCodeHandlers = _statusCodeHandlers ??
                                 new ConcurrentDictionary<HttpStatusCode, Func<ErrorStatusContext, Task<bool>>>();

            if (_statusCodeHandlers.ContainsKey(code))
            {
                throw new InvalidOperationException(
                    $"The status code: {code} already has a registered handler.");
            }

            _statusCodeHandlers[code] = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <summary>
        /// Registers a object responsible for serializing and deserializing for a given media type.
        /// </summary>
        /// <typeparam name="T">The type of the serializer.</typeparam>
        /// <param name="mediaType">The media type value associated with serializer.  If not specified,
        /// the media type returned from the provided serializer type is used.</param>
        public RequestClientBuilder UsingMediaTypeSerializer<T>(string mediaType = null)
            where T : IMediaTypeSerializer, new()
        {
            _mediaTypeSerializers = _mediaTypeSerializers ?? new Dictionary<string, IMediaTypeSerializer>();

            var serializer = new T();
            mediaType = mediaType ?? serializer.MediaType;

            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException("Client base address not specified.", nameof(mediaType));

            if (_mediaTypeSerializers.ContainsKey(mediaType))
            {
                throw new InvalidOperationException(
                    $"A media type serializer for media type: {mediaType} is already registered.");
            }

            _mediaTypeSerializers[mediaType] = serializer;
            return this;
        }
  
        /// <summary>
        /// Returns an instance of the request client built using the specified configurations.
        /// </summary>
        /// <returns>Created request client.</returns>
        public IRequestClient Build()
        {
            AssureDefaultSerializers();

            var settings = _defaultRequestSettings ?? RequestSettings.Create(config => config.UseHalDefaults());

            _logger = (_loggerFactory ?? new LoggerFactory()).CreateLogger<RequestClient>();
            
            // Create an instance of the RequestClient that delegates to the MS HttpClient.
            var httpClient = new HttpClient { BaseAddress = new Uri(_baseAddressUri) };
            if (_clientSettings.Timeout != null)
            {
                httpClient.Timeout = _clientSettings.Timeout.Value;
            }

            var requestClient = new RequestClient(httpClient, _logger, _mediaTypeSerializers, settings);

            // If the configuration specified a service entry point path, configure a delegate to load
            // the resource of first successful response.
            if (_entryPointPath != null)
            {
                _apiEntryPointLazy = new Lazy<Task<HalEntryPointResource>>(
                    () => GetEntryPointResource(requestClient), true);

                requestClient.SetApiServiceProvider(this);
            }

            // If the configuration specified a method to invoke before each request, pass it on to the client.
            if (_eachRequestAction != null)
            {
                requestClient.SetEachRequestAction(_eachRequestAction);
            }

            if (_correlationIdHeaderName != null)
            {
                requestClient.AddCorrelationId(_correlationIdHeaderName);
            }

            if (_statusCodeHandlers != null)
            {
                requestClient.SetStatusCodeHandlers(_statusCodeHandlers);
            }

            return requestClient;
        }

        // Invoked by the created request client when the entry point is requested.
        Task<HalEntryPointResource> IServiceEntryApiProvider.GetEntryPointResource()
        {
            if (_apiEntryPointLazy == null)
            {
                throw new InvalidOperationException(
                    $"The request client with the base address: {_baseAddressUri} does not have " +
                     "entry point path specified.");
            }

            return _apiEntryPointLazy.Value;
        }

        private static void InitServicePointManager(string baseAddress, ClientSettings settings) 
        {
            var servicePoint = ServicePointManager.FindServicePoint(new Uri(baseAddress));

            if (settings.ConnectionLeaseTimeout != null) 
            {
                servicePoint.ConnectionLeaseTimeout = settings.ConnectionLeaseTimeout.Value;
            }

            if (settings.ConnectionLimit != null) 
            {
                servicePoint.ConnectionLimit = settings.ConnectionLimit.Value;
            }

            if (settings.DnsRefreshTimeout != null)
            {
                ServicePointManager.DnsRefreshTimeout = settings.DnsRefreshTimeout.Value;
            }
        }

        // Attempts to load the entry point resource and resets the lazy loaded state if there
        // is an exception so future attempts can be made.
        private async Task<HalEntryPointResource> GetEntryPointResource(IRequestClient requestClient)
        {
            try
            {
                var request = ApiRequest.Get(_entryPointPath);
                var response = await requestClient.SendAsync<HalEntryPointResource>(request);

                response.ThrowIfNotSuccessStatusCode();
                return response.Content;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error loading service entry resource");
                _apiEntryPointLazy = new Lazy<Task<HalEntryPointResource>>(() => GetEntryPointResource(requestClient), true);
                throw;
            }
        }

        private void AssureDefaultSerializers()
        {
            if (_mediaTypeSerializers != null) return;

            UsingMediaTypeSerializer<JsonMediaTypeSerializer>();
            UsingMediaTypeSerializer<JsonMediaTypeSerializer>(InternetMediaTypes.HalJson);
        }
    }
}
