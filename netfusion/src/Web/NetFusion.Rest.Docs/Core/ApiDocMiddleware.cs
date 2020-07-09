using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.Common;
using NetFusion.Rest.Docs.Models;
using NetFusion.Rest.Docs.Plugin;

namespace NetFusion.Rest.Docs.Core
{
    /// <summary>
    /// ASP.NET pipeline middleware that will respond to requests for documentation describing an Api hosted at a
    /// specific URL.  The Url sent in the request is the relative Url stored within the route metadata populated
    /// by Microsoft.  This corresponds to the action's route template (the value of the Url containing parameters
    /// tokens) without any value substitution.  For example, to query the documentation for the Url:
    ///
    ///     http://localhost:5000/api/customers/10/accounts/top/5
    ///
    /// the following would be sent for the documentation:
    ///     api/customers/{id}/accounts/top/{count}
    ///
    /// Assuming the default endpoint used to query the documentation is used, the following would be the
    /// complete request:
    ///
    /// http://localhost:5000/api/net-fusion/rest?doc=api/customers/{id}/accounts/top/{count}
    ///
    /// When using the NetFusion.Rest.Server plugin to return Link Relations associated with resources,
    /// the caller can specify the following header value to return the route template to use when
    /// requesting documentation.  When this header is specified, the "docQuery" link property contains
    /// the template version of the Url used to query documentation.
    ///
    ///     include-url-for-doc-query: yes
    ///
    /// "_links": {
    ///     "update": {
    ///         "href": "/api/schools/students/9361c8b2-a8cf-4320-b8a5-d565575d0780",
    ///         "docQuery": "api/schools/students/{id}",
    ///         "templated": false,
    ///         "methods": ["POST"]
    ///     }
    /// </summary>
    public class ApiDocMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDocModule _docModule;
        
        public ApiDocMiddleware(RequestDelegate next, IDocModule docModule)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _docModule = docModule ?? throw new ArgumentNullException(nameof(docModule));
        }
        
        public async Task Invoke(HttpContext httpContext, IApiDocService apiDocService)
        {
            // Determine if the request matches the requirements for a query
            // used to request documentation for a specific Web Api.
            if (! IsDocumentationRequest(httpContext, out string relativeDocPath))
            {
                await _next(httpContext);
                return;
            }
            
            // If there is documentation for the requested Web Api route, return 
            // response containing the documentation.
            if (apiDocService.TryGetActionDoc(relativeDocPath, out ApiActionDoc actionDoc))
            {
                await RespondWithApiDoc(httpContext.Response, actionDoc);
                return;
            }

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }
        
        private bool IsDocumentationRequest(HttpContext context, out string relativeDocPath)
        {
            relativeDocPath = null;
            
            string path = context.Request.Path.Value;
            if (! path.Equals(_docModule.RestDocConfig.EndpointUrl, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
    
            IQueryCollection query = context.Request.Query;
            if (query.TryGetValue("doc", out var url) && !string.IsNullOrWhiteSpace(url))
            {
                relativeDocPath = url;
                return true;
            }

            return false;
        }

        private async Task RespondWithApiDoc(HttpResponse response, ApiActionDoc actionDoc)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = InternetMediaTypes.Json;

            await response.WriteAsync(JsonSerializer.Serialize(
                actionDoc, 
                _docModule.RestDocConfig.SerializerOptions), Encoding.UTF8);
        }
    }
}