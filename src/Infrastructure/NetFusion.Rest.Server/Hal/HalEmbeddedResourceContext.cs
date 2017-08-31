using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Modules;
using System.Linq;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Can be used by service components to determine the embedded resources
    /// requested by the client.  If the client didn't specify, the service
    /// should return all default embedded resources.  The service is not 
    /// required to support this feature.
    /// </summary>
    public class HalEmbeddedResourceContext : IHalEmbededResourceContext
    {
        private readonly IActionContextAccessor _contextAccessor;
        private readonly IResourceMediaModule _resourceModule;

        public HalEmbeddedResourceContext(
            IActionContextAccessor contextAccessor,
            IResourceMediaModule resourceModule)
        {
            _contextAccessor = contextAccessor;
            _resourceModule = resourceModule;
        }

        public bool IsResourceRequested<TResource>() where TResource : IResource
        { 
            // If not specified by the caller, all resources are considered requested.
            if (!EmbeddedResourcesRequested)
            {
                return true;
            }

            var mappedResourceName = _resourceModule.GetMappedResourceName(typeof(TResource));
            return RequestedEmbeddedResources.Contains(mappedResourceName);
        }

        private bool EmbeddedResourcesRequested => _contextAccessor.ActionContext
            .HttpContext.Request.Query.ContainsKey("embed");

        public string[] RequestedEmbeddedResources
        {
            get
            {
                IQueryCollection query =_contextAccessor.ActionContext.HttpContext.Request.Query;
                StringValues queryValue;

                if (query.TryGetValue("embed", out queryValue))
                {
                    return queryValue[0].Split(',');
                }
                return new string[] { };
            }
        }
    }
}
