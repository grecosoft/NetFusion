using System;
using Microsoft.AspNetCore.Builder;

namespace NetFusion.Web.Rest.CodeGen.Plugin;

/// <summary>
/// Extension methods for IApplicationBuilder that can be used
/// when configuring a WebApi service.
/// </summary>
public static class RestCodeGenExtensions
{
    /// <summary>
    /// Adds a middleware component to the ASP.NET Core pipeline called
    /// to obtain the corresponding TypeScript generated code.
    /// </summary>
    /// <param name="builder">The ASP.NET Core application builder.</param>
    /// <returns>Application Builder</returns>
    public static IApplicationBuilder UseRestCodeGen(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseMiddleware<ApiCodeGenMiddleware>();
        return builder;
    }
}