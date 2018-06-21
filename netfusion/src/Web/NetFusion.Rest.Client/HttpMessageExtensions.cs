using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace NetFusion.Rest.Client
{
    public static class HttpMessageExtensions
    {
        /// <summary>
        /// Determines if the response is an authentication challenge containing the
        /// realm URL for authentication.
        /// </summary>
        /// <param name="response">The response from a HTTP call.</param>
        /// <returns>True if an authentication challenge.  Othersize, False.</returns>
        public static bool IsAuthChallenge(this HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            
            return response.StatusCode == HttpStatusCode.Unauthorized && response.Headers
                       .GetValues("WWW-Authenticate").Any(v => v.StartsWith("realm"));
        }

        /// <summary>
        /// If an authentication challenge, returns the the URL to be called to
        /// obtain an authentication token.
        /// </summary>
        /// <param name="response">The response from a HTTP call.</param>
        /// <returns>The URL to call to authenticate.</returns>
        public static string GetAuthRealmUrl(this HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            
            if ( ! response.IsAuthChallenge())
            {
                throw new InvalidOperationException(
                    "WWW-Authenticate header not found.");
            }

            return response.Headers.GetValues("WWW-Authenticate")
                .First(v => v.StartsWith("realm"))
                .Replace("realm=", "");
        }

        /// <summary>
        /// Return the X-Custom-Token header value.
        /// </summary>
        /// <param name="response">The http response.</param>
        /// <returns>The token value if present.  Otherwise, null is returned.</returns>
        public static string GetAuthToken(this ApiResponse response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            
            if (response.IsSuccessStatusCode && response.Headers.TryGetValues(
                    "X-Custom-Token", out IEnumerable<string> values))
            {
                return values.First();
            }

            return null;
        }
    }
}