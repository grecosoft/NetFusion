using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.Client;
using NetFusion.Rest.Client.Core;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;

namespace WebTests.Hosting
{
    /// <summary>
    /// Provides method used to act on the create TestServer by executing web-requests.
    /// </summary>
    public class WebServerAct
    {
        private readonly TestServer _testServer;
        private readonly IServiceProvider _services;
  
        public WebServerAct(TestServer testServer, IServiceProvider services)
        {
            _testServer = testServer ?? throw new ArgumentNullException(nameof(testServer));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }
        
        /// <summary>
        /// Creates an HttpClient instance used to act on the TestService by sending requests.
        /// </summary>
        /// <param name="clientAct">Delegate passed the HttpClient to be acted on.</param>
        /// <returns>The WebServer response to be asserted.</returns>
        public async Task<WebServerResponse> OnClient(Func<HttpClient, Task<HttpResponseMessage>> clientAct)
        {
            if (clientAct == null) throw new ArgumentNullException(nameof(clientAct));
            
            var client = _testServer.CreateClient();
            var httpResponseMsg = await clientAct(client);

            return new WebServerResponse(_services, httpResponseMsg);
        }

        /// <summary>
        /// Creates a Request-Client instance used to act on the TEstService by sending requests.
        /// </summary>
        /// <param name="clientAct">Delegate passed the IRequestClient to be acted on.</param>
        /// <returns>The WebServer response to be asserted.</returns>
        public async Task<WebServerResponse> OnRestClient( 
            Func<IRequestClient, Task<ApiResponse>> clientAct)
        {
            if (clientAct == null) throw new ArgumentNullException(nameof(clientAct));
            
            var client = _testServer.CreateClient();
            var logger = _services.GetService<ILogger<WebServerAct>>();

            var settings = new ClientSettings
            {
                SerializerOptions = new JsonSerializerOptions
                {
                    IgnoreNullValues = true, 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            };
            
            var jsonSerializer = new JsonMediaTypeSerializer();
            jsonSerializer.Initialize(settings);

            var serializers = new Dictionary<string, IMediaTypeSerializer>
            {
                { InternetMediaTypes.Json, jsonSerializer },
                { InternetMediaTypes.HalJson, jsonSerializer }
            };
            var restClient = new RequestClient(client, logger, serializers, 
                RequestSettings.Create( c => c.UseHalDefaults()));

            var apiResponse = await clientAct(restClient);
            
            return new WebServerResponse(_services, apiResponse);
        }    
    }
}