using NetFusion.Rest.Client.Settings;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using NetFusion.Rest.Client.Resources;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Represents a request that can be submitted to the server.
    /// </summary>
    public class ApiRequest
    {
        /// <summary>
        /// The Uri to be used when making the request.
        /// </summary>
        public string RequestUri { get; private set; }

        /// <summary>
        /// Indicates that the RequestUri contains template tokens.
        /// </summary>
        public bool IsTemplate { get; private set; }

        /// <summary>
        /// The method to be used when making the request.
        /// </summary>
        public HttpMethod Method { get; private set; }

        /// <summary>
        /// Set to indicate to the server what embedded resources should
        /// be returned.  This allows the client select a sub set of the
        /// possible embedded resources.  If not specified, the server 
        /// will return all embedded resources.
        /// </summary>
        public string EmbeddedNames { get; private set; }

        /// <summary>
        /// The content of the request-body to be serialized and sent with the request.
        /// </summary>
        public object Content { get; private set; }

        /// <summary>
        /// The request settings specific to the request.  These settings are merged
        /// into any default settings specified by the client used to make the request.
        /// </summary>
        public IRequestSettings Settings { get; private set; }

        /// <summary>
        /// Creates a new request for a specified Uri for a given HTTP method.
        /// </summary>
        /// <param name="requestUri">The request Uri to call.</param>
        /// <param name="method">The HTTP method used when calling Uri.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
		public static ApiRequest Create(string requestUri, HttpMethod method, Action<ApiRequest> config = null)
        {
            var request = new ApiRequest
            {
				RequestUri = requestUri,
			    Method = method,
                IsTemplate = IsTemplateUrl(requestUri)
            };

            config?.Invoke(request);

			request.AssertRequest();
            return request;
        }

        /// <summary>
        /// Creates a new GET request to a specified Uri.
        /// </summary>
        /// <param name="requestUri">The request Uri to call.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest Get(string requestUri, Action<ApiRequest> config = null)
        {
            return Create(requestUri, HttpMethod.Get, config);
        }

        /// <summary>
        /// Creates a new POST request to a specified Uri.
        /// </summary>
        /// <param name="requestUri">The request Uri to call.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest Post(string requestUri, Action<ApiRequest> config = null)
        {
            return Create(requestUri, HttpMethod.Post, config);
        }

        /// <summary>
        /// Creates a new PUT request to a specified Uri.
        /// </summary>
        /// <param name="requestUri">The request Uri to call.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest Put(string requestUri, Action<ApiRequest> config = null)
        {
            return Create(requestUri, HttpMethod.Put);
        }

        /// <summary>
        /// Creates a new DELETE request to a specified Uri.
        /// </summary>
        /// <param name="requestUri">The request Uri to call.</param>
        /// <param name="config">Optional delegate passed the created ApiRequest used to apply 
        /// additional configurations.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest Delete(string requestUri, Action<ApiRequest> config = null)
        {
            return Create(requestUri, HttpMethod.Delete, config);
        }

        /// <summary>
        /// Creates a new request based on a provided link.
        /// </summary>
        /// <param name="link">The link from which to build the request.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest Create(Link link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link),
                "Link not specified.");

            return CreateFromLink(link.Href, link);
        }
        
        /// <summary>
        /// Creates a new request based on a provided link and route values.
        /// </summary>
        /// <param name="link">The link from which to build the request.</param>
        /// <param name="tokens">The value used to replace any route tokens.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest Create(Link link, IDictionary<string, object> tokens)
        {
            if (link == null) throw new ArgumentNullException(nameof(link),
                "Link cannot be null.");

            if (tokens == null) throw new ArgumentNullException(nameof(tokens),
                "Route value dictionary cannot be null.");

            // Note: The href of the Link must not be set to the URL after replacing tokens since
            // template links need to be able to be reused.
            string href = link.Href;
            if (link.Templated)
            {
                href = ReplaceTemplateTokensWithValues(href, tokens);
            }

            return CreateFromLink(href, link);
        }

        /// <summary>
        /// Creates a new request based on a provided link and route values.
        /// </summary>
        /// <param name="link">The link form which to build to request.</param>
        /// <param name="tokens">The values used to replace any route tokens.</param>
        /// <returns>Created API request.</returns>
        public static ApiRequest Create(Link link, Action<dynamic> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens),
                "Values delegate cannot be null.");

            var expando = new ExpandoObject();
            tokens(expando);
            return Create(link, expando);
        }

        private static ApiRequest CreateFromLink(string href, Link link)
        {
            if (IsTemplateUrl(href))
            {
                throw new InvalidOperationException(
                    $"Links containing non-populated template URL tokens can't be requested.");
            }

            AssertLink(link);

            var request = new ApiRequest
            {
                RequestUri = href,
                Method = new HttpMethod(link.Methods.First()),
                IsTemplate = false
            };

            request.AssertRequest();
            return request;
        }

        /// <summary>
        /// Specific settings to be used for the request.  These settings are merged
        /// into the default setting of the IRequestClient instance.
        /// </summary>
        /// <param name="settings">The request specific settings.</param>
        /// <returns>The API request instance updated with the request specific settings.</returns>
        public ApiRequest UsingSettings(IRequestSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings), 
                "Settings cannot be null.");

            return this;
        }

        /// <summary>
        /// The content to be sent to the server when the request is executed.
        /// </summary>
        /// <param name="content">The content to be serialized as the request body.</param>
        /// <returns>The API request instance updated with the content.</returns>
        public ApiRequest WithContent(object content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content), 
                "Content cannot be null.");

            return this;
        }

        /// <summary>
        /// Indicates to the server which named resources it should embed.
        /// This allows only a sub set of the embedded resourced to be returned.
        /// The names are just hints to the server allowing it to return only a
        /// sub set of the resources it would have normally returned.  The server
        /// is not obligated to implemented this feature.
        /// </summary>
        /// <returns>New link instance.</returns>
        /// <param name="names">Names.</param>
        public ApiRequest Embed(params string[] names)
        {
            EmbeddedNames = string.Join(",", names);
            return this;
        }

        /// <summary>
        /// Specifies the values used to replace URI template tokens.
        /// </summary>
        /// <param name="tokens">The values used to replace the route tokens.</param>
        /// <returns>Updated API Request.</returns>
        public ApiRequest WithRouteValues(IDictionary<string, object> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens),
                "Route value dictionary cannot be null.");

            RequestUri = ReplaceTemplateTokensWithValues(RequestUri, tokens);
            IsTemplate = IsTemplateUrl(RequestUri);
            return this;
        }

        /// <summary>
        /// Specifies the values used to replace URI template tokens.
        /// </summary>
        /// <param name="tokens">The values used to replace the route tokens.</param>
        /// <returns>Updated API Request.</returns>
        public ApiRequest WithRouteValues(Action<dynamic> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens),
                "Values delegate cannot be null.");

            var expando = new ExpandoObject();
            tokens(expando);

            return WithRouteValues(expando);
        }

        //----------------- STATE ASSERTIONS ---------------

        private void AssertRequest()
		{
			if (string.IsNullOrWhiteSpace(RequestUri))
			{
				throw new InvalidOperationException("Request URI value not specified.");
			}

			if (Method == null)
			{
				throw new InvalidOperationException("Request HTTP Method not specified.");
			}
		}

		private static void AssertLink(Link link)
		{
			if (!link.Methods.Any())
			{
				throw new InvalidOperationException($"Link Method value not specified for Href: {link.Href}.");
			}

			if (link.Methods.Count() > 1)
			{
				throw new InvalidOperationException(
					$"More then one Link Method value specified for Href: {link.Href}.");
			}
		}

        // ----------------- TEMPLATE POPULATION --------------------
        private static string ReplaceTemplateTokensWithValues(string urlTemplate, IDictionary<string, object> routeValues)
        {
            foreach (var routeValue in routeValues)
            {
                string routeKey = routeValue.Key;

                urlTemplate = ReplaceRouteTokens(urlTemplate, routeKey, routeValue.Value);

                routeKey = routeKey[0].ToString().ToLower() + routeKey.Substring(1, routeKey.Length - 1);
                urlTemplate = ReplaceRouteTokens(urlTemplate, routeKey, routeValue.Value);
            }

            return RemoveOptionalRouteTokens(urlTemplate);
        }

        private static string ReplaceRouteTokens(string urlTemplate, string routeKey, object routeValue)
        {
            return urlTemplate
                .Replace("{" + routeKey + "}", routeValue.ToString())
                .Replace("{?" + routeKey + "}", routeValue.ToString());
        }

        private static bool IsTemplateUrl(string url)
        {
            return url?.Contains("/{") ?? false;
        }

        private static string RemoveOptionalRouteTokens(string populatedUrl)
        {
            return populatedUrl?.Split(new[] { "/{?" }, StringSplitOptions.RemoveEmptyEntries)[0];
        }
    }
}
