using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Client.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Factory for creating instances of type IResouceClient.  At the beginning 
    /// of the application's bootstrap process, one or more base addresses can be
    /// added to the factory by calling the RegisterBaseAddress method.
    /// </summary>
    public class RequestClientFactory : IDisposable
    {
        /// <summary>
        /// Get the singleton instance of the ClientFactory.
        /// </summary>
        public static RequestClientFactory Instance { get; }

        private bool _disposed;

        private IDictionary<string, IRequestSettings> _baseAddressRegistrations;  // BaseAddress => DefaultSettings
        private ConcurrentDictionary<string, IRequestClient> _resourceClients;    // BaseAddress => RequestClient
        private IDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;

        static RequestClientFactory()
        {
            Instance = new RequestClientFactory();
        }

        private RequestClientFactory()
        {
            _baseAddressRegistrations = new Dictionary<string, IRequestSettings>();
            _mediaTypeSerializers = new Dictionary<string, IMediaTypeSerializer>();

            _resourceClients = new ConcurrentDictionary<string, IRequestClient>();
        }

        /// <summary>
        /// Called at the beginning of the consuming application to register base
        /// API addresses for which IResourceClient instances should be created.
        /// </summary>
        /// <param name="baseAddress">The base address to associated with the client.</param>
        /// <param name="settings">The default settings applied when making a request.</param>
        public void RegisterBaseAddress(string baseAddress, IRequestSettings requestSettings)
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
                throw new ArgumentException("Client base address not specified.", nameof(baseAddress));

            if (_baseAddressRegistrations.ContainsKey(baseAddress))
            {
                throw new InvalidOperationException(
                    $"The base address: {baseAddress} is already registered.");
            }

            _baseAddressRegistrations[baseAddress] = requestSettings ?? throw new ArgumentNullException(nameof(requestSettings),
                "Default client settings not specified.");
        }

        /// <summary>
        /// Registers a object responsible for serializing and deserializing for a given media type.
        /// </summary>
        /// <typeparam name="T">The type of the serializer.</typeparam>
        /// <param name="mediaType">The media type value associated with serializer.  If not specified,
        /// the media type returned from the provided serializer type is used.</param>
        public void RegisterMediaTypeSerializer<T>(string mediaType = null)
            where T : IMediaTypeSerializer, new()
        {
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
        }

        /// <summary>
        /// Returns the IResourceClient implementation associated with the specified base address.
        /// </summary>
        /// <returns>The client instance associated with the specified base address.</returns>
        /// <param name="baseAddress">The base address for the associated client.</param>
        public IRequestClient GetClient(string baseAddress)
        {
            if (string.IsNullOrWhiteSpace(baseAddress))
                throw new ArgumentException("Base address not specified.", nameof(baseAddress));

            return _resourceClients.GetOrAdd(baseAddress, CreateResourceClient);
        }

        private IRequestClient CreateResourceClient(string baseAddress)
        {

            // Lookup the default setting to use for the specified base-address.
            if (!_baseAddressRegistrations.TryGetValue(baseAddress, out IRequestSettings settings))
            {
                throw new ArgumentException(
                    $"The base address: {baseAddress} is not registered.", nameof(baseAddress));
            }

            // Create instance of the ResourceClient that will delegate to the MS HttpClient.
            HttpClient httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            return new RequestClient(httpClient, _mediaTypeSerializers, settings);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;

            foreach (HttpClient httpClient in _resourceClients.Values)
            {
                httpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
