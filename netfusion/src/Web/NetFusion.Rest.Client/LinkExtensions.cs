using System;
using System.Collections.Generic;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Extension methods used to create a request from link object.
    /// </summary>
    public static class LinkExtensions
    {
        /// <summary>
        /// Creates a request corresponding to the link.
        /// </summary>
        /// <param name="link">The link to create a request from.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest ToRequest(this Link link, Action<ApiRequest> config = null)
        {
            var request = ApiRequest.Create(link);
            
            config?.Invoke(request);
            return request;
        }

        /// <summary>
        /// Creates a request corresponding to the link.
        /// </summary>
        /// <param name="link">The link to create a request from.</param>
        /// <param name="tokens">The values to use for link route template parameters.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest ToRequest(this Link link, IDictionary<string, object> tokens,
            Action<ApiRequest> config = null)
        {
            var request = ApiRequest.Create(link, tokens);
            
            config?.Invoke(request);
            return request;
        }

        /// <summary>
        /// Creates a request corresponding to the link.
        /// </summary>
        /// <param name="link">The link to create a request from.</param>
        /// <param name="tokens">The values to use for link route template parameters.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest ToRequest(this Link link, Action<dynamic> tokens,
            Action<ApiRequest> config = null)
        {
            var request = ApiRequest.Create(link, tokens);
            
            config?.Invoke(request);
            return request;
        }
    }
}
