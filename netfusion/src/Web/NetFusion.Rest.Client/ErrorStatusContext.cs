using System;
using System.Net.Http;
using System.Threading.Tasks;
using NetFusion.Rest.Client.Settings;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Contains references used to retry a request resulting in an error.
    /// </summary>
    public class ErrorStatusContext
    {
        public IRequestClient Client { get; }
        public HttpResponseMessage Response { get; }
        public IRequestSettings DefaultSettings { get; set; }
        public IRequestSettings Settings { get; }
        
        public ErrorStatusContext (
            IRequestClient client,
            HttpResponseMessage response,
            IRequestSettings defaultSettings,
            IRequestSettings requestSettings)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Response = response ?? throw new ArgumentNullException(nameof(response));
            DefaultSettings = defaultSettings ?? throw new ArgumentNullException(nameof(defaultSettings));
            Settings = requestSettings ?? throw new ArgumentNullException(nameof(requestSettings));
        }

        /// <summary>
        /// Used to login with basic authentication if the response is an
        /// authentication challenge by calling the returned Realm URL.
        /// If the login attempt is returned an HTTP 200 status code, the
        /// JWT authentication token is added to the Settings of the context
        /// to be used when retrying the request.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Result of the authentication.</returns>
        public async Task<AuthResult> AttemptLogin(string username, string password)
        {
            if (! Response.IsAuthChallenge())
            {
                return new AuthResult();
            }

            // Initialize settings to invoke auth service:
            var settings = RequestSettings.Create(config =>
            {
                config.SuppressStatusCodeHandlers = true;
                config.Headers.SetBasicAuthHeader(username, password);
            });

            // Sent a GET request to the Realm URL returned in the HTTP 401 challenge:
            var request = ApiRequest.Get(Response.GetAuthRealmUrl())
                .UsingSettings(settings);

            var response = await Client.SendAsync(request);
            var authToken = response.GetAuthToken();
            
            // If the response was a HTTP 200 and returned a token,
            // set the token on the default RequestClient settings
            // so it will be used for all future requests.
            if (response.IsSuccessStatusCode && authToken != null)
            {
                DefaultSettings.Headers.SetAuthBearerToken(authToken);
                return new AuthResult(authToken);
            }

            return new AuthResult();
        }
    }

    public class AuthResult
    {
        public bool IsSuccess { get; }
        public string Token { get; }

        public AuthResult()
        {
            IsSuccess = false;
        }

        public AuthResult(string token)
        {
            IsSuccess = true;
            Token = token;
        }
    }
}