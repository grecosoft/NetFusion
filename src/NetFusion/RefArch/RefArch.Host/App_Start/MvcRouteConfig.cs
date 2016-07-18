using System.Web.Mvc;
using System.Web.Routing;

namespace Samples.WebHost.App_Start
{
    public class MvcRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "App", action = "NetFusionPortal" }
            );
        }
    }
}
