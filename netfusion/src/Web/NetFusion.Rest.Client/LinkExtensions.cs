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
        /// <returns>Created API request.</returns>
        public static ApiRequest ToRequest(this Link link)
        {
            return ApiRequest.Create(link);
        }

        /// <summary>
        /// Creates a request corresponding to the link.
        /// </summary>
        /// <param name="link">The link to create a request from.</param>
        /// <param name="tokens">The values to use for link route template parameters.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest ToRequest(this Link link, IDictionary<string, object> tokens)
        {
            return ApiRequest.Create(link, tokens);
        }

        /// <summary>
        /// Creates a request corresponding to the link.
        /// </summary>
        /// <param name="link">The link to create a request from.</param>
        /// <param name="tokens">The values to use for link route template parameters.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest ToRequest(this Link link, Action<dynamic> tokens)
        {
            return ApiRequest.Create(link, tokens);
        }
    }
}
