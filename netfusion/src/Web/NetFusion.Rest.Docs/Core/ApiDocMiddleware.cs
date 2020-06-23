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
    public class ApiDocMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDocModule _docModule;
        
        
        public ApiDocMiddleware(RequestDelegate next, IDocModule docModule)
        {
            _next = next;
            _docModule = docModule;
        }
        
        public async Task Invoke(HttpContext httpContext, IApiDocService apiDocService)
        {
            // Determine if the request matches requirements for a request
            // for documentation for a specific Web Api method.
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
            
            // Not a documentation specific request, call next middleware component.
            await _next(httpContext);
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
            if (query.ContainsKey("doc"))
            {
                relativeDocPath = query["doc"];
            }

            return !string.IsNullOrWhiteSpace(relativeDocPath);
        }

        private async Task RespondWithApiDoc(HttpResponse response, ApiActionDoc actionDoc)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = InternetMediaTypes.Json;

            await response.WriteAsync(JsonSerializer.Serialize(
                actionDoc, 
                _docModule.RestDocConfig.SerializerOptions ), Encoding.UTF8);
        }
    }
}