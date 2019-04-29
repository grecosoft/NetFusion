using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Container;
using NetFusion.Common.Extensions;
using NetFusion.Web.Mvc.Metadata.Core;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Extends the MVC application builder with a method called to 
    /// specify the base URL that can be called to obtain route metadata.
    /// NOTE:  This is not required if the metadata is only queried on the server.
    /// </summary>
    public static class BuilderExtensions
    {
        private const string DefaultUrl = "api/netfusion/route/metadata";

        /// <summary>
        /// Called when configuring an MVC application to specify
        /// the URL at which route metadata can be queried.
        /// </summary>
        /// <param name="app">The MVC application being extended.</param>
        /// <param name="baseUrl">The base URL used to query the metadata.</param>
        /// <returns>Application Builder</returns>
        public static IApplicationBuilder UseRouteMetadata(this IApplicationBuilder app, 
            string baseUrl = DefaultUrl)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (baseUrl.Trim().Length == 0)
            {
                throw new ArgumentException("URL not specified");
            }
           
            ConfigureMetadataRoutes(app, baseUrl);
            return app;
        }

        private static void ConfigureMetadataRoutes(IApplicationBuilder app, string baseUrl)
        {
            app.UseMvc(routes =>
            {
                routes.MapGet(baseUrl + "/groups", context =>
                {
                    using (var scope = CompositeContainer.Instance.CreateServiceScope())
                    {
                        var metadataSrv = scope.ServiceProvider.GetService<IApiMetadataService>();
                        var metadata = metadataSrv.GetApiGroups();
                        return SetResponse(context.Response, metadata);
                    }
                });

                routes.MapGet(baseUrl + "/groups/{groupName}", context =>
                {
                    using (var scope = CompositeContainer.Instance.CreateServiceScope())
                    {
                        var metadataSrv = scope.ServiceProvider.GetService<IApiMetadataService>();
                        var groupName = context.GetRouteValue("groupName").ToString();
                        var metadata = metadataSrv.GetApiGroup(groupName);
                        return SetResponse(context.Response, metadata);
                    }
                });
            });
        }

        private static Task SetResponse(HttpResponse response, ApiGroupMeta[] metadata)
        {
            if (metadata.Length == 0)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            var model = MetadataMap.GetModel(metadata);

            response.ContentType = "application/json";
            return response.WriteAsync(model.ToJson());
        }
    }
}
