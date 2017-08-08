using Microsoft.AspNetCore.Http;
using NetFusion.Common;

namespace NetFusion.Web.Mvc.Extensions
{
    public static class HttpContextExtensions
    {
        public static T GetService<T>(this HttpContext httpContext)
        {
            Check.NotNull(httpContext, nameof(HttpContext));

            return (T)httpContext.RequestServices.GetService(typeof(T));
        }
    }
}
