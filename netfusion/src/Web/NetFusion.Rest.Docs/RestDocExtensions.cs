using Microsoft.AspNetCore.Builder;
using NetFusion.Rest.Docs.Core;

namespace NetFusion.Rest.Docs
{
    public static class RestDocExtensions
    {
        public static IApplicationBuilder UseRestDocs(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ApiDocMiddleware>();
            return builder;
        }
    }
}