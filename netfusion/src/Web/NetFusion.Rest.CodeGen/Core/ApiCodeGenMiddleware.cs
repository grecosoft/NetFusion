using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetFusion.Rest.CodeGen.Plugin;
using NetFusion.Rest.Common;

namespace NetFusion.Rest.CodeGen.Core
{
    public class ApiCodeGenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICodeGenModule _codeGenModule;

        public ApiCodeGenMiddleware(RequestDelegate next, ICodeGenModule codeGenModule)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _codeGenModule = codeGenModule ?? throw new ArgumentNullException(nameof(codeGenModule));
        }

        public async Task Invoke(HttpContext httpContext, IApiCodeGenService apiCodeGenService)
        {
            // If the request is not for a generate REST Api file,
            // invoke the next request delegate in the pipline.
            if (! IsCodeGenRequest(httpContext, out string resourceName))
            {
                await _next(httpContext);
                return;
            }

            // Since this is a request for a generated code file, determine if the
            // file with the specified resource-name exists.
            if (apiCodeGenService.TryGetResourceCodeFile(resourceName, out Stream stream))
            {
                await RespondWithApiDoc(httpContext.Response, stream);
                return;
            }

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        }

        private bool IsCodeGenRequest(HttpContext context, out string resourceName)
        {
            resourceName = null;

            string path = context.Request.Path.Value;
            if (!path.Equals(_codeGenModule.CodeGenConfig.EndpointUrl, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            IQueryCollection query = context.Request.Query;
            if (query.TryGetValue("resource", out var resource) && !string.IsNullOrWhiteSpace(resource))
            {
                resourceName = resource;
            }

            return resourceName != null;
        }
        private async Task RespondWithApiDoc(HttpResponse response, Stream stream)
        {
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = InternetMediaTypes.PlainText;

            using var streamReader = new StreamReader(stream);
            string contents = await streamReader.ReadToEndAsync();

            await response.WriteAsync(contents, Encoding.UTF8);
        }
    }
}
