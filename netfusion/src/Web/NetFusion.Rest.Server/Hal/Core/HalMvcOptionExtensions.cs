using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NetFusion.Rest.Server.Hal.Core
{
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
        /// <returns></returns>
        public static MvcOptions UseHalFormatter(this MvcOptions mvcOptions, JsonSerializerOptions options = null)
        {
            if (mvcOptions == null) throw new ArgumentNullException(nameof(mvcOptions), 
                "Options reference not specified.");

            // Use default settings if not specified by caller.
            options ??= new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Add the HAL formatter to the MVC pipeline.
            mvcOptions.OutputFormatters.Add(new HalJsonOutputFormatter(options));
            return mvcOptions;
        }
    }
}
