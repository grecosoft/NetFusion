
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Generation.Core
{
    /// <summary>
    /// Filter that will short-circuit other filters and returns the type-script definitions
    /// for the controller's action method being invoked.
    /// </summary>
    public class ResourceTypeFilter : IAsyncResourceFilter
    {
        private IHostingEnvironment _hostingEnvironment;
        private IResourceTypeReader _resourceReader;

        public ResourceTypeFilter(
            IHostingEnvironment hostingEnvironment,
            IResourceTypeReader resourceReader)
        {
            _hostingEnvironment = hostingEnvironment;
            _resourceReader = resourceReader;
        }

        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (IsTypeScriptDefinitionRequest(context))
            {
                return ReadTypeDefinitions(context);
            }

            return next();
        }

        private bool IsTypeScriptDefinitionRequest(ResourceExecutingContext context)
        {
            IQueryCollection query = context.HttpContext.Request.Query;
            return query.ContainsKey("types") && query["types"] == "true";
        }

        private async Task ReadTypeDefinitions(ResourceExecutingContext context)
        {
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            MethodInfo actionMethod = actionDescriptor.MethodInfo;

            MemoryStream memoryStream = await _resourceReader.ReadTypeDefinitionFiles(actionMethod);
            
            var contentDisposition = new ContentDispositionHeaderValue("attachment");
            contentDisposition.SetHttpFileName("resource-types.ts");
            context.HttpContext.Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();

            context.Result = new FileStreamResult(memoryStream, new MediaTypeHeaderValue("text/plain"));
        }
    }
}
