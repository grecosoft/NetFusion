using Microsoft.AspNetCore.Mvc.Filters;
using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions;

namespace WebApiHost.Filters
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var x = (context.Exception as NetFusionException).Details.ToIndentedJson();
        }
    }
}
