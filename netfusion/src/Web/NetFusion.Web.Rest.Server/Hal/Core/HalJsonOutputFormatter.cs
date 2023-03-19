using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Web.Common;
using NetFusion.Web.Extensions;
using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Resources;
using NetFusion.Web.Rest.Server.Mappings;
using NetFusion.Web.Rest.Server.Plugin;

namespace NetFusion.Web.Rest.Server.Hal.Core;

/// <summary>
/// Output formatter that checks if the response object is of type HalResource and
/// adds the HAL resource metadata.  Then the resulting resource is formatted as JSON.
/// </summary>
public class HalJsonOutputFormatter : TextOutputFormatter
{
    public JsonSerializerOptions SerializerOptions { get; }

    public HalJsonOutputFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        SerializerOptions = jsonSerializerOptions 
                            ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));

        SupportedMediaTypes.Clear();
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(InternetMediaTypes.HalJson));
        SupportedEncodings.Add(Encoding.UTF8);
        SupportedEncodings.Add(Encoding.Unicode);
    }
        
    protected override bool CanWriteType(Type? type)
    {
        return type?.CanAssignTo<HalResource>() ?? false;
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context,
        Encoding selectedEncoding)
    {
        if (context.Object == null)
        {
            return;
        }

        var resource = (HalResource)context.Object;
       
        ResourceContext resourceContext = CreateContext(resource, context.HttpContext);
        ApplyMetadataToResource(resource, resourceContext);
            
        // Delegate to the Json Serializer to produce HAL+JSON.
        await JsonSerializer.SerializeAsync(context.HttpContext.Response.Body, 
            context.Object, 
            context.Object.GetType(),
            SerializerOptions);
    }
        
    private static void ApplyMetadataToResource(HalResource resource, ResourceContext resourceContext)
    {
        resourceContext.Resource = resource;
        resourceContext.Model = resource.GetModel();
            
        resourceContext.MediaModule.ApplyResourceMeta(InternetMediaTypes.HalJson, resourceContext);

        // Add metadata to each embedded resource if present.
        if (resource.Embedded == null)
        {
            return;
        }

        foreach (object embeddedResource in resource.Embedded.Values)
        {
            // Check if the embedded resource is a collection of resources and if so
            // apply the metadata to each contained resource.
            if (embeddedResource is IEnumerable resourceColl)
            {
                foreach (HalResource childResource in resourceColl.OfType<HalResource>())
                {
                    ApplyMetadataToResource(childResource, resourceContext);
                }

                continue;
            }

            // Add metadata to single embedded resource.
            if (embeddedResource is HalResource halResource)
            {
                ApplyMetadataToResource(halResource, resourceContext);
            }
        }
    }

    private static ResourceContext CreateContext(HalResource resource, HttpContext httpContext) =>
        new (
            httpContext,
            resource,
            httpContext.GetService<IResourceMediaModule>(),
            httpContext.GetService<IApiMetadataService>(),
            httpContext.GetService<IUrlHelper>(),
            httpContext.GetService<ILogger<HalJsonOutputFormatter>>());
}