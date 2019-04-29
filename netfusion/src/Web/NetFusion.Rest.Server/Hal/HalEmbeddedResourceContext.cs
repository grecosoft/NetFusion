using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using NetFusion.Rest.Resources;
using System.Linq;
using NetFusion.Rest.Server.Plugin;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Can be used by service components to determine the embedded resources requested by the client.  
    /// If the client didn't specify, the service should return all default embedded resources.  The 
    /// service is not required to support this feature.
    /// </summary>
    public class HalEmbeddedResourceContext : IHalEmbeddedResourceContext
    {
        private readonly IActionContextAccessor _contextAccessor;
        private readonly IResourceMediaModule _resourceModule;

        public HalEmbeddedResourceContext(
            IActionContextAccessor contextAccessor,
            IResourceMediaModule resourceModule)
        {
            _contextAccessor = contextAccessor ?? throw new System.ArgumentNullException(nameof(contextAccessor));
            _resourceModule = resourceModule ?? throw new System.ArgumentNullException(nameof(resourceModule));
        }

        public bool IsResourceRequested<TResource>() where TResource : IResource
        { 
            // If not specified by the caller, all resources are considered requested.
            if (! EmbeddedResourcesRequested)
            {
                return true;
            }

            var mappedResourceName = _resourceModule.GetMappedResourceName(typeof(TResource));
            return RequestedEmbeddedResources.Contains(mappedResourceName);
        }

        // The client can specify an embed query-string parameter to indicate to the server
        // the embedded resources they are interested in.  This is optional but can be used
        // by the client and the server to reduce network traffic.
        private bool EmbeddedResourcesRequested => _contextAccessor.ActionContext
            .HttpContext.Request.Query.ContainsKey("embed");

        public string[] RequestedEmbeddedResources
        {
            get
            {
                IQueryCollection query =_contextAccessor.ActionContext.HttpContext.Request.Query;

                if (query.TryGetValue("embed", out StringValues queryValue))
                {
                    return queryValue[0].Replace(" ", string.Empty).Split(',');
                }
                return new string[] { };
            }
        }
    }
}
