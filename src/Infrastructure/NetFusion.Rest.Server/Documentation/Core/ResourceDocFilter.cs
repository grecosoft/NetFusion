using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Documentation.Core
{
    /// <summary>
    /// Filter that intercepts the pipeline and checks for the "doc" query string parameter.  
    /// If present, the actual controller's action method is not invoked and documentation
    /// for the action method is returned.
    /// </summary>
    public class ResourceDocFilter : IAsyncResourceFilter
    {
        private readonly IDocReader _docReader;

        public ResourceDocFilter(IDocReader docReader)
        {
            _docReader = docReader;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (IsDocumentationRequest(context))
            {
				var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

				MethodInfo actionMethod = actionDescriptor.MethodInfo;
                IHeaderDictionary headers = context.HttpContext.Request.Headers;
                
                DocActionModel actionDocModel = await _docReader.GetActionDocModel(headers, actionMethod);
                if (actionDocModel == null)
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
                }
                else
                {           
                    context.Result = new ObjectResult(actionDocModel);
                }
            }
            else
            {
                await next();
            }
        }

		private bool IsDocumentationRequest(ResourceExecutingContext context)
		{
			IQueryCollection query = context.HttpContext.Request.Query;
			return query.ContainsKey("doc") && query["doc"] == "true";
		}
    }
}
