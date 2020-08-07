using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Implements a factory for creating RestClient instances.  The created instance
    /// wraps the HttpClient and adds additional features.
    /// </summary>
    public class RestClientFactory : IRestClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;
        
        public RestClientFactory(
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IEnumerable<IMediaTypeSerializer> mediaTypeSerializers)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            
            if (mediaTypeSerializers == null) throw new ArgumentNullException(nameof(mediaTypeSerializers));
            
            _mediaTypeSerializers = CreateSerializerLookup(mediaTypeSerializers.ToArray());
        }

        public IRestClient CreateClient(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name of configured API not specified.", nameof(name));
            
            ILogger logger = _loggerFactory.CreateLogger(name);

            // Create instance of HttpClient and wrap it within a RestClient instance.
            HttpClient innerClient = _httpClientFactory.CreateClient(name);
            IRestClient client = new RestClient(logger, innerClient, _mediaTypeSerializers);

            return client;
        }
        
        private static IDictionary<string, IMediaTypeSerializer> CreateSerializerLookup(
            IMediaTypeSerializer[] serializers)
        {
            var duplicated = serializers.WhereDuplicated(s => s.MediaType).ToArray();
            if (duplicated.Any())
            {
                throw new InvalidOperationException(
                    $"More than one {typeof(IMediaTypeSerializer)} is registered for the " + 
                    $"following media-types: {string.Join(',', duplicated)}.");
            }

            return serializers.ToDictionary(s => s.MediaType);
        }
    }
}