using System;
using System.Reflection;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using NetFusion.WebApi.Metadata;
using System.Linq;
using System.Web.Http.Controllers;

namespace NetFusion.WebApi.Core
{
    /// <summary>
    /// Service that exposes WebApi route meta-data that can be used by client to invoke
    /// endpoints routes without have the URL hard-coded in their code.  Each WebApi
    /// controller specifies the EndpointMetadata attribute with the name the endpoint.
    /// </summary>
    public class RouteMetadataService : IRouteMetadataService
    {
        private IEnumerable<EndpointMetadata> _routeMetadata = null;

        private IApiExplorer ApiExplorer
        {
            get { return GlobalConfiguration.Configuration.Services.GetApiExplorer(); }
        }

        public IDictionary<string, EndpointMetadata> GetApiMetadata()
        {
            return QueryControllerMeta().ToDictionary(cm => cm.EndpointName);
        }

        public IDictionary<string, EndpointMetadata> GetApiMetadata<T>()
            where T : ApiController
        {
            return QueryControllerMeta().Where(m => m.ControllerType == typeof(T))
                .ToDictionary(cm => cm.EndpointName);
        }

        public IDictionary<string, EndpointMetadata> GetApiMetadataInNamespace<T>()
            where T : ApiController
        {
            return QueryControllerMeta().Where(m => m.ControllerType.Namespace == typeof(T).Namespace)
                .ToDictionary(cm => cm.EndpointName);
        }

        private IEnumerable<EndpointMetadata> QueryControllerMeta()
        {
            if (this.ApiExplorer == null)
            {
                throw new NullReferenceException(
                    $"The {nameof(IApiExplorer)} service implementation is null.");
            }

            if (_routeMetadata == null)
            {
                _routeMetadata = this.ApiExplorer.ApiDescriptions
                    .GroupBy(k => k.ActionDescriptor.ControllerDescriptor)
                    .Select(kv => new EndpointMetadata
                    {
                        ControllerType = kv.Key.ControllerType,
                        EndpointName = GetEndpointName(kv.Key),
                        RouteMetadata = QueryRouteMeta(kv)
                    });
            }

            return _routeMetadata;
        }

        private string GetEndpointName(HttpControllerDescriptor controllerDesc)
        {
            var controllerMetaAttr = GetControllerMetaAttr(controllerDesc);
            return controllerMetaAttr?.EndpointName ?? controllerDesc.ControllerName;
        }

        private EndpointMetadataAttribute GetControllerMetaAttr(HttpControllerDescriptor controllerDesc)
        {
            return controllerDesc.ControllerType.GetCustomAttribute<EndpointMetadataAttribute>();
        }

        private IDictionary<string, RouteMetadata> QueryRouteMeta(IEnumerable<ApiDescription> apiDescs)
        {
            // If an action method has mutiple HTTP methods, the first one will be selected.
            // The assigned HTTP methods can be read from the child SupportsMethods property.
            var distinctActions = apiDescs.Where(IsExposed).GroupBy(a => a.ActionDescriptor.ActionName)
                .Select(g => g.First());

            return distinctActions.ToDictionary(
                k => k.ActionDescriptor.ActionName,
                v => new RouteMetadata
                {
                    Template = v.RelativePath,
                    Methods = QuerySupportedMethods(v.ActionDescriptor),
                    ParamMetadata = QueryParamMeta(v.ActionDescriptor)
                });
        }

        private bool IsExposed(ApiDescription apiDesc)
        {
            var controllerAttr = GetControllerMetaAttr(apiDesc.ActionDescriptor.ControllerDescriptor);
            var routeAttr = GetRouteMetaAttr(apiDesc.ActionDescriptor);

            if (controllerAttr?.IncluedAllRoutes ?? false)
            {
                return routeAttr?.IncludeRoute ?? true;
            }
            else
            {
                return routeAttr?.IncludeRoute ?? false;
            }
        }

        private RouteMetadataAttribute GetRouteMetaAttr(HttpActionDescriptor actionDesc)
        {
            return actionDesc.GetCustomAttributes<RouteMetadataAttribute>().FirstOrDefault();
        }

        private IEnumerable<string> QuerySupportedMethods(HttpActionDescriptor actionDesc)
        {
            return actionDesc.SupportedHttpMethods.Select(sm => sm.Method);
        }

        private IEnumerable<ParameterMetadata> QueryParamMeta(HttpActionDescriptor actionDesc)
        {
            return actionDesc.GetParameters()
                .Select(p => new ParameterMetadata
                {
                    Name = p.ParameterName,
                    IsOptional = p.IsOptional,
                    Type = GetParamJsType(p),
                    DefaultValue = p.DefaultValue
                });
        }

        private string GetParamJsType(HttpParameterDescriptor paramDesc)
        {
            TypeCode typeCode = Type.GetTypeCode(paramDesc.ParameterType);

            // TODO:
            return null;
        }


    }
}
