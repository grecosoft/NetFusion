using System;
using Microsoft.AspNetCore.Builder;
using NetFusion.Rest.CodeGen.Core;

namespace NetFusion.Rest.CodeGen.Plugin
{
    public static class RestCodeGenExtensions
    {
        public static IApplicationBuilder UseRestCodeGen(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.UseMiddleware<ApiCodeGenMiddleware>();
            return builder;
        }
    }
}
