using Autofac;
using System.Web.Http;

namespace NetFusion.WebApi
{
    /// <summary>
    /// Implemented by a plug-in module to be called when an WebApi application
    /// is ready for configuration.
    /// </summary>
    public interface IWebApiConfiguration
    {
        /// <summary>
        /// Called by the WebApi module, and implemented by other plug-in modules,
        /// to be given the opportunity to configure any WebApi runtime features.
        /// </summary>
        /// <param name="config">The HTTP configuration.</param>
        /// <param name="container">The container that was built by AppContainer.</param>
        void OnConfigureWebApiReady(HttpConfiguration config, IContainer container);
    }
}
