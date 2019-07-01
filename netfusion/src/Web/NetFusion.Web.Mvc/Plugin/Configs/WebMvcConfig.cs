using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Web.Mvc.Plugin.Configs
{
    /// <summary>
    /// General MVC configuration containing overall settings.
    /// </summary>
    public class WebMvcConfig : IPluginConfig
    {
        /// <summary>
        /// Determines if route metadata can be queried by consumers.
        /// </summary>
        public bool EnableRouteMetadata { get; set; }    
    }
}
