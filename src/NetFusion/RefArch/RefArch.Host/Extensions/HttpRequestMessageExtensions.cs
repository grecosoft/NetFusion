using System.Net;
using System.Net.Http;
using System.Web.Http.Routing;

namespace RefArch.Host.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpResponseMessage Authenticated(this HttpRequestMessage request,
            object authModel,
            string token)
        {
            var urlHelper = new UrlHelper(request);
            var response = request.CreateResponse(HttpStatusCode.OK, authModel);
            response.Headers.Add("X-Custom-Token", token);

            return response;
        }
    }
}