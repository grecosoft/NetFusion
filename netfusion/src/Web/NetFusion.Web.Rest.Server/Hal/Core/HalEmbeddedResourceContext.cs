using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace NetFusion.Web.Rest.Server.Hal.Core;

/// <summary>
/// Can be used by service components to determine the embedded resource models requested by the client.  
/// If the client didn't specify, the service should return all default embedded resources models.  The 
/// service is not required to support this feature.
/// </summary>
public class HalEmbeddedResourceContext : IHalEmbeddedResourceContext
{
    private ActionContext ActionContext { get; }

    public HalEmbeddedResourceContext(IActionContextAccessor contextAccessor)
    {
        if (contextAccessor == null) throw new ArgumentNullException(nameof(contextAccessor));

        ActionContext = contextAccessor.ActionContext ?? 
                        throw new NullReferenceException("Action Context not Initialized.");
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
    private bool EmbeddedResourcesRequested => ActionContext
        .HttpContext.Request.Query.ContainsKey("embed");

    public string[] RequestedEmbeddedModels
    {
        get
        {
            IQueryCollection query = ActionContext.HttpContext.Request.Query;

            if (query.TryGetValue("embed", out StringValues queryValue) && queryValue.Count == 1)
            {
                string value = queryValue[0] ?? string.Empty;
                return value.Replace(" ", string.Empty).Split(',');
            }
            return Array.Empty<string>();
        }
    }
}