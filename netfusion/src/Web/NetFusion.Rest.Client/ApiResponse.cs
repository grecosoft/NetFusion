using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Contains result properties for a submitted request.
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// The message for which the result is associated.
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// Indicates if the response code is considered successful.
        /// </summary>
        public bool IsSuccessStatusCode { get; }

        /// <summary>
        /// The HTTP status code returned from the server.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// A descriptive phase usually related with the status code.
        /// </summary>
        public string ReasonPhase { get; }

        /// <summary>
        /// The response headers.
        /// </summary>
        public HttpResponseHeaders Headers { get;  }

        /// <summary>
        /// The media-type of the returned response content.
        /// Null if the response didn't contain content.
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        /// The character set of the returned response content.
        /// Null if the response didn't contain content.
        /// </summary>
        public string CharSet { get; }

        /// <summary>
        /// The length of the returned response content.
        /// Null if the response didn't contain content.
        /// </summary>
        public long? ContentLength { get; }

        /// <summary>
        /// Contains information about the server to which the request was submitted.
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// Returns the value of the ETag header.
        /// </summary>
        public string ETag { get; }

        private HttpResponseMessage _responseMsg;

        public ApiResponse(HttpRequestMessage requestMsg, HttpResponseMessage responseMsg)
        {
            Request = requestMsg ?? throw new ArgumentNullException(nameof(requestMsg),
                "Request Message cannot be null.");

            _responseMsg = responseMsg ?? throw new ArgumentNullException(nameof(responseMsg),
                "Response Message cannot be null.");
            
            IsSuccessStatusCode = responseMsg.IsSuccessStatusCode;
            StatusCode = responseMsg.StatusCode;
            ReasonPhase = responseMsg.ReasonPhrase;
            Headers = responseMsg.Headers;

            HttpContent httpContent = responseMsg.Content;

            ContentLength = httpContent?.Headers.ContentLength;
            MediaType = httpContent?.Headers.ContentType?.MediaType;
            CharSet = httpContent?.Headers.ContentType?.CharSet;

            ETag = responseMsg.Headers.ETag?.Tag;
            Server = responseMsg.Headers.Server?.ToString();
        }

        /// <summary>
        /// Allows the consumer handling the response to throw an HttpRequestException
        /// if a success HTTP status code was not received.
        /// </summary>
        public void ThrowIfNotSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Response status code does not indicate success: {(int)StatusCode} ({StatusCode})");
            }
        }
    }

	/// <summary>
    /// Contains result properties for a submitted request and the response content returned from the server.
	/// </summary>
	public class ApiResponse<TContent> : ApiResponse
        where TContent : class
    {
        public TContent Content { get; }

        public ApiResponse(HttpRequestMessage requestMsg, HttpResponseMessage responseMsg, TContent content)
            : base(requestMsg, responseMsg)
        {
            Content = content;
        }        
    }
}
