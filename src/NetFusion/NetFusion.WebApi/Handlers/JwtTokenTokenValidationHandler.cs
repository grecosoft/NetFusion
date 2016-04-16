using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace NetFusion.WebApi.Handlers
{
    /// <summary>
    /// WebApi pipeline handler that checks the request for JWT authentication token
    /// header.  Sets the principal if the user is authenticated and invokes the 
    /// WebApi method requested. 
    /// </summary>
    public class JwtTokenValidationHandler : DelegatingHandler
    {
        private readonly IJwtTokenService _jwtTokenSrv;

        public JwtTokenValidationHandler(IJwtTokenService jwtTokenSrv)
        {
            _jwtTokenSrv = jwtTokenSrv;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var headers = request.Headers;

                if (headers.Authorization != null && headers.Authorization.Scheme.Equals("Bearer"))
                {
                    string jwtToken = request.Headers.Authorization.Parameter;
                    var hostPrincipal = _jwtTokenSrv.CreatePrincipal(jwtToken);
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = hostPrincipal;
                    }
                }

                // Call WebApi method.
                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer",
                        "error=\"invalid_token\""));
                }
                return response;
            }
            catch
            {
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "error=\"invalid_token\""));
                return response;
            }
        }
    }
}