using NetFusion.Rest.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NetFusion.Rest.Client.Settings
{
    /// <summary>
    /// HTTP Headers associated with a HTTP request.
    /// </summary>
    public class RequestHeaders
    {
		/// <summary>
		/// The HTTP accept media-type. 
		/// </summary>
		public HeaderValue[] Accept { get; private set; }

		/// <summary>
		/// The HTTP content media-type.
		/// </summary>
		public HeaderValue ContentType { get; private set; }

		/// <summary>
		/// A named list of header values associated with the request.
		/// </summary>
		public IReadOnlyDictionary<string, HeaderValue> Headers { get; }

		private readonly IDictionary<string, HeaderValue> _headers;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RequestHeaders()
		{
			_headers = new Dictionary<string, HeaderValue>();
			Headers = new ReadOnlyDictionary<string, HeaderValue>(_headers);
		}

        /// <summary>
        /// Adds a header value to be associated with the request.
        /// </summary>
        /// <returns>Self reference for method chaining.</returns>
        /// <param name="name">The name of the header value.</param>
        /// <param name="value">The value of the header.</param>
        public RequestHeaders Add(string name,  params string[] value)
        {
			if (String.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Header name not specified.", nameof(name));

            if (IsKnownHeaderName(name))
            {
                throw new ArgumentException(
                    $"The HTTP Header named: {name} must be set using the specific " + 
                    $"header method defined on the {this.GetType()} class.", 
                    nameof(name));
            }

            _headers[name] = HeaderValue.WithValue(value);
            return this;
        }

        private bool IsKnownHeaderName(string name)
        {
            return new[] { HttpHeaderNames.Accept, HttpHeaderNames.ContentType }.Contains(name);
        }

        /// <summary>
        /// Sets the Accept media-type header value.  To accept multiple media types,
        /// this method can be called multiple times.
        /// </summary>
        /// <returns>Self reference for method chaining.</returns>
        /// <param name="value">The media-type value.</param>
        /// <param name="quality">The precedence of the value compared in relation to 
        /// other accept media-type values.</param>
        public RequestHeaders AcceptMediaType(string value, double? quality = null)
        {
			var acceptValues = Accept?.ToList() ?? new List<HeaderValue>();
			acceptValues.Add(HeaderValue.WithValue(value, quality));

			Accept = acceptValues.ToArray();
            return this;
        }

        /// <summary>
        /// Sets the content media-type value.
        /// </summary>
        /// <returns>Self reference for method chaining.</returns>
        /// <param name="value">The media-type value.</param>
        public RequestHeaders ContentMediaType(string value)
        {
            ContentType = HeaderValue.WithValue(value);
            return this;
        }

		internal void ApplyHeaders(HttpRequestMessage requestMessage)
		{
			if (Accept != null)
			{
                foreach(HeaderValue acceptHeader in Accept)
                {
                    string acceptType = acceptHeader.Value.First();

                    var mediaTypeHeader = acceptHeader.Quality == null ? 
                        new MediaTypeWithQualityHeaderValue(acceptType) : 
                        new MediaTypeWithQualityHeaderValue(acceptType, acceptHeader.Quality.Value);

                    requestMessage.Headers.Accept.Add(mediaTypeHeader);
                }
			}

			foreach (var header in Headers)
			{
                requestMessage.Headers.Add(header.Key, header.Value.Value);
			}
		}

		internal RequestHeaders GetMergedHeaders(RequestHeaders requestSettings)
		{
			// Check of any of the known header values have been overridden.
			var mergedHeaders = new RequestHeaders
			{
				Accept = requestSettings.Accept ?? Accept ?? new[] { HeaderValue.WithValue (InternetMediaTypes.Json) },
				ContentType = requestSettings.ContentType ?? ContentType ?? HeaderValue.WithValue(InternetMediaTypes.Json)
            };

			// Add all the headers from this settings object to the new merged version.
			foreach (var header in Headers)
			{
                mergedHeaders._headers[header.Key] = header.Value;
			}

			// Override or add any header values being merged.
			foreach (var header in requestSettings.Headers)
			{
				mergedHeaders._headers[header.Key] = header.Value;
			}

			return mergedHeaders;
		}
    }
}
