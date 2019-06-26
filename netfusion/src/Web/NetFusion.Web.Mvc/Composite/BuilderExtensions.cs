using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Common.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NetFusion.Web.Mvc.Composite
{
    /// <summary>
    /// Extends the MVC application builder with a method called to 
    /// query the composite log of the application showing how it was
    /// constructed from a set of plugins.
    /// </summary>
    public static class BuilderExtensions
    {
        private const string DefaultUrl = "api/netfusion/composite";

        /// <summary>
        /// Called when configuring an MVC application to specify the URL 
        /// at which application composite information can be read.
        /// </summary>
        /// <param name="app">The MVC application being extended.</param>
        /// <param name="baseUrl">The base URL used to query the composite structure.</param>
        /// <returns>Application Builder</returns>
        public static IApplicationBuilder UseCompositeQuerying(this IApplicationBuilder app,
            string baseUrl = DefaultUrl)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (baseUrl.Trim().Length == 0)
            {
                throw new ArgumentException("URL not specified");
            }


            ConfigureCompositeRoutes(app, baseUrl);

            return app;
        }

        private static void ConfigureCompositeRoutes(
            IApplicationBuilder app,
            string baseUrl)
        {
            app.UseMvc(routes =>
            {
                routes.MapGet(baseUrl + "/structure", context =>
                {
                    using (var scope = CompositeApp.Instance.CreateServiceScope())
                    {
                        var compositeSrv = scope.ServiceProvider.GetRequiredService<ICompositeService>();
                        var compositeModel = compositeSrv.GetStructure();

                        return SetResponse(context.Response, compositeModel);
                    }
                });

                routes.MapGet(baseUrl + "/plugins/{pluginId}", context =>
                {
                    var pluginId = context.GetRouteValue("pluginId").ToString();

                    using (var scope = CompositeApp.Instance.CreateServiceScope())
                    {
                        var compositeSrv = scope.ServiceProvider.GetRequiredService<ICompositeService>();
                        var pluginDetailModel = compositeSrv.GetPluginDetails(pluginId);
                        return SetResponse(context.Response, pluginDetailModel);
                    }
                });
            });
        }

        private static Task SetResponse(HttpResponse response, object compositeModel)
        {
            if (compositeModel == null)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            response.ContentType = "application/json";
            return response.WriteAsync(compositeModel.ToJson());
        }
    }
}
