using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NetFusion.Rest.Docs.Models;

namespace NetFusion.Rest.Docs.Core
{
    public class ApiDocFilter : IAsyncResourceFilter
    {
        private readonly IApiDocService _docService;

        public ApiDocFilter(IApiDocService docService)
        {
            _docService = docService;
        }
        
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (! IsDocumentationRequest(context))
            {
                await next();
                return;
            }

            MethodInfo actionMethod = GetActionMethodInfo(context);
            if (_docService.TryGetActionDoc(actionMethod, out ApiActionDoc apiActionDoc))
            {
                context.Result = new ObjectResult(apiActionDoc);
            }
            else
            {
                context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
            }
        }
        
        private static bool IsDocumentationRequest(ActionContext context)
        {
            IQueryCollection query = context.HttpContext.Request.Query;
            return query.ContainsKey("doc") && query["doc"] == "true";
        }

        private static MethodInfo GetActionMethodInfo(ActionContext context)
        {
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            return actionDescriptor.MethodInfo;
        }
    }
}