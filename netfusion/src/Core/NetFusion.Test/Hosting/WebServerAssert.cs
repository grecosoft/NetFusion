using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Rest.Client;

namespace NetFusion.Test.Hosting
{
    /// <summary>
    /// Provides methods for asserting the request made to the TestServer.
    /// </summary>
    public class WebServerAssert
    {
        private readonly IServiceProvider _services;
        private readonly HttpResponseMessage _httpResponse;
        private readonly ApiResponse _apiResponse;
        
        public WebServerAssert(IServiceProvider services,
            HttpResponseMessage httpResponse,
            ApiResponse apiResponse)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _httpResponse = httpResponse;
            _apiResponse = apiResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assert"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public WebServerAssert HttpResponse(Action<HttpResponseMessage> assert)
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert));

            if (_httpResponse == null)
            {
                throw new InvalidOperationException("HttpClient was not acted on.");
            }

            assert(_httpResponse);
            return this;
        }
        
        public WebServerAssert ApiResponse(Action<ApiResponse> assert)
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert));

            if (_apiResponse == null)
            {
                throw new InvalidOperationException("IRequestClient as not acted on.");
            }

            assert(_apiResponse);
            return this;
        }
        
        public WebServerAssert Service<TService>(Action<TService> assert)
        {
            if (assert == null) throw new ArgumentNullException(nameof(assert));

            var instance = _services.GetRequiredService<TService>();
            assert(instance);
            
            return this;
        }
    }
}