using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Common;

namespace NetFusion.Web.Mvc
{
    /// <summary>
    /// General MVC configuration containing overall settings.
    /// </summary>
    public class WebMvcConfig : IContainerConfig
    {
        /// <summary>
        /// MVC Service collection.
        /// </summary>
        public IServiceCollection Services { get; private set; }

        /// <summary>
        /// Determines if route metadata can be queried by consumers.
        /// </summary>
        public bool EnableRouteMetadata { get; set; }

        /// <summary>
        /// Specifies the MVC services collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        public void UseServices(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));
            this.Services = services;
        }

        public void Validate()
        {
            if (this.Services == null)
            {
                throw new ContainerException(
                    $"Service collection not specified for configuration: {nameof(WebMvcConfig)}");
            }
        }
    }
}
