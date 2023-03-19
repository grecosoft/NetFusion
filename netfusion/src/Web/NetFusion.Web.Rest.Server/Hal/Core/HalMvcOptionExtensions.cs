using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace NetFusion.Web.Rest.Server.Hal.Core;

/// <summary>
/// Contains extension methods that can be used when initializing a MVC.NET Core
/// web application host.
/// </summary>
public static class HalMvcOptionExtensions
{
    /// <summary>
    /// Adds formatter to the pipeline that checks for HAL based resources and
    /// determines the links that should be returned. 
    /// </summary>
    /// <param name="mvcOptions">The MVC options passed from Web API host.</param>
    /// <param name="options">The optional JSON serialization settings to use.</param>
    /// <returns>MVC options.</returns>
    public static MvcOptions UseHalFormatter(this MvcOptions mvcOptions, JsonSerializerOptions? options = null)
    {
        if (mvcOptions == null) throw new ArgumentNullException(nameof(mvcOptions), 
            "Options reference not specified.");

        // Use default settings if not specified by caller.
        var serializationOptions = options ?? new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Add the HAL formatter to the MVC pipeline.
        mvcOptions.OutputFormatters.Add(new HalJsonOutputFormatter(serializationOptions));
        return mvcOptions;
    }
}