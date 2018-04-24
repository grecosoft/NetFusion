using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Client.Resources;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using System;
using System.Collections.Generic;
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
        private readonly string _baseAddressUri;
        private string _entryPointPath;

        private IRequestSettings _defaultRequestSettings;
        private IDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;
        private Lazy<Task<HalEntryPointResource>> _apiEntryPointLazy;

        private Action<IRequestSettings> _eachRequestAction;

        private RequestClientBuilder(string baseAddressUri)
        {
            if (string.IsNullOrWhiteSpace(baseAddressUri))
                throw new ArgumentException("Base address must be specified.", nameof(baseAddressUri));

            _baseAddressUri = baseAddressUri;
        }

        /// <summary>
        /// Creates a new builder for a specific base service Uri.
        /// </summary>
        /// <param name="baseAddressUrl">The base address to which the client will send requests.</param>
        /// <returns>Instance of the builder class.</returns>
        public static RequestClientBuilder ForBaseAddress(string baseAddressUrl)
        {
            return new RequestClientBuilder(baseAddressUrl);
        }

        /// <summary>
        /// Sets a child path of the base address that is to be invoked to load the 
        /// root service api methods to which the client can communicated.  All other
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
        /// Registers a delegate that will be invoked before each request.
        /// </summary>
        /// <param name="settings">Reference to the request settings being used for the current request.</param>
        /// <returns>Build for method chaining.</returns>
        public RequestClientBuilder BeforeEachRequest(Action<IRequestSettings> settings)
        {
            _eachRequestAction = settings ?? throw new ArgumentNullException(nameof(settings));
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
            var httpClient = new HttpClient { BaseAddress = new Uri(_baseAddressUri) };
            var requestClient = new RequestClient(httpClient, _mediaTypeSerializers, settings);

            // If the configuration specified a service entry point path, configure a delegate to load
            // the resource of first successful response.
            if (_entryPointPath != null)
            {
                _apiEntryPointLazy = new Lazy<Task<HalEntryPointResource>>(() => GetEntryPointResource(requestClient), true);
                requestClient.SetApiServiceProvider(this);
            }

            // If the configuration specified a method to invoke before each request, pass it on to the client.
            if (_eachRequestAction != null)
            {
                requestClient.SetEachRequestAction(_eachRequestAction);
            }
            return requestClient;
        }

        // Invoked by the created request client when the entry point is requested.
        public Task<HalEntryPointResource> GetEntryPointResource()
        {
            if (_apiEntryPointLazy == null)
            {
                throw new InvalidOperationException(
                    $"The request client with the base address: {_baseAddressUri} does not have " +
                    $"entry point path specified.");
            }

            return _apiEntryPointLazy.Value;
        }

        // Attempts to load the entry point resource and resets the lazy loaded state if there
        // is an exception so future attempts can be made.
        private async Task<HalEntryPointResource> GetEntryPointResource(IRequestClient requestClient)
        {
            try
            {
                var request = ApiRequest.Get(_entryPointPath);
                var response = await requestClient.SendAsync<HalEntryPointResource>(request);
                return response.Content;
            }
            catch (Exception ex) // TODO:  Pass in logger...
            {
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
