using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Contains extension methods that can be used when initializing a MVC.NET Core
    /// web application host.
    /// </summary>
    public static class HalMvcOptionExtensions
    {
        /// <summary>
        /// Adds formatter to the pipeline that checks for a resources and determines the
        /// links that should be returned. 
        /// </summary>
        /// <param name="options">The MVC options passed from Web API host.</param>
        /// <param name="settings">The optional JSON serialization settings to use.</param>
        /// <returns></returns>
        public static MvcOptions UseHalFormatter(this MvcOptions options, JsonSerializerSettings settings = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), "Options reference not specified.");

            // Use default settings if not specified by caller.
            settings = settings ?? new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore };

            // Add the HAL formatter to the MVC pipeline.
            options.OutputFormatters.Add(new HalFormatter(settings, ArrayPool<Char>.Create()));
            return options;
        }
    }
}
