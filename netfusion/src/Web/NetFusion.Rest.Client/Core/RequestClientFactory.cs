using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Rest.Client.Settings;

namespace NetFusion.Rest.Client.Core
{
    public class RequestClientFactory : IRequestClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDictionary<string, IMediaTypeSerializer> _mediaTypeSerializers;

        
        public RequestClientFactory(
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory,
            IEnumerable<IMediaTypeSerializer> mediaTypeSerializers)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            
            if (mediaTypeSerializers == null) throw new ArgumentNullException(nameof(mediaTypeSerializers));
            
            _mediaTypeSerializers = CreateSerializerLookup(mediaTypeSerializers.ToArray());
        }

        public IRequestClient CreateClient(string name)
        {
            ILogger logger = _loggerFactory.CreateLogger<RequestClient>();
            
            HttpClient innerClient = _httpClientFactory.CreateClient(name);
            IRequestClient client = new RequestClient(logger, innerClient, _mediaTypeSerializers);

            return client;
        }

        private static IDictionary<string, IMediaTypeSerializer> CreateSerializerLookup(
            IMediaTypeSerializer[] serializers)
        {
            var duplicated = serializers.WhereDuplicated(s => s.MediaType);
            if (duplicated.Any())
            {
                
            }

            return serializers.ToDictionary(s => s.MediaType);

        }
    }
}