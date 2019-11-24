using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using NetFusion.Rest.Resources;
using System.Linq;
using NetFusion.Rest.Server.Resources;

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

        public HalEmbeddedResourceContext(IActionContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public bool IsRequested<TResource>() where TResource : IResource
        { 
            // If not specified by the caller, all resources are considered requested.
            if (! EmbeddedResourcesRequested)
            {
                return true;
            }

            var resourceName = typeof(TResource).GetExposedResourceTypeName();
            return resourceName != null && RequestedEmbeddedResources.Contains(resourceName);
        }

        public bool IsRequested(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource key name must be specified.", nameof(resourceName));
            
            return RequestedEmbeddedResources.Contains(resourceName);
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
