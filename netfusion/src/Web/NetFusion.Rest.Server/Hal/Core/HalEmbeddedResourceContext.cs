using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace NetFusion.Rest.Server.Hal.Core
{
    /// <summary>
    /// Can be used by service components to determine the embedded resource models requested by the client.  
    /// If the client didn't specify, the service should return all default embedded resources models.  The 
    /// service is not required to support this feature.
    /// </summary>
    public class HalEmbeddedResourceContext : IHalEmbeddedResourceContext
    {
        private readonly IActionContextAccessor _contextAccessor;

        public HalEmbeddedResourceContext(IActionContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public bool IsRequested(string embeddedName)
        {
            if (string.IsNullOrWhiteSpace(embeddedName))
                throw new ArgumentException("Embedded name must be specified.", nameof(embeddedName));
            
            // If not specified by the caller, all resources are considered requested.
            return !EmbeddedResourcesRequested || RequestedEmbeddedModels.Contains(embeddedName);
        }

        // The client can specify the embed query-string parameter to indicate to the server
        // the embedded resource models they are interested in.  This is optional but can be
        // used by the client and the server to reduce network traffic based on the client type.
        private bool EmbeddedResourcesRequested => _contextAccessor.ActionContext
            .HttpContext.Request.Query.ContainsKey("embed");

        public string[] RequestedEmbeddedModels
        {
            get
            {
                IQueryCollection query = _contextAccessor.ActionContext.HttpContext.Request.Query;

                if (query.TryGetValue("embed", out StringValues queryValue))
                {
                    return queryValue[0].Replace(" ", string.Empty).Split(',');
                }
                return new string[] { };
            }
        }
    }
}
