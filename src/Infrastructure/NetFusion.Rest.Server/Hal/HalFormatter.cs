using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Rest.Common;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Resources.Hal;
using NetFusion.Rest.Server.Mappings;
using NetFusion.Rest.Server.Modules;
using NetFusion.Web.Mvc.Extensions;
using NetFusion.Web.Mvc.Metadata;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Output formatter that checks if the response object is of type IHalResource and
    /// adds the resource metadata.
    /// </summary>
    public class HalFormatter : JsonOutputFormatter
    {
        public HalFormatter(JsonSerializerSettings settings, ArrayPool<char> charPool) : base(settings, charPool)
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(InternetMediaTypes.HalJson));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return type.CanAssignTo<IHalResource>();
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context.Object is IHalResource resource)
            {
                ResourceContext resourceContext = CreateContext(context.HttpContext);
                ApplyMetadataToResource(resource, resourceContext);
            }

            // Delegate to the base implementation to serialize the updated response object as JSON
            // returning HAL-JSON representation.
            return base.WriteResponseBodyAsync(context, selectedEncoding);
        }

        private void ApplyMetadataToResource(IHalResource resource, ResourceContext resourceContext)
        {
            resourceContext.Resource = resource;
            resourceContext.MediaModule.ApplyResourceMeta(InternetMediaTypes.HalJson, resourceContext);

            // Add metadata to each embedded resource if present.
            if (resource.Embedded == null)
            {
                return;
            }

            foreach (IResource embeddedResource in resource.Embedded.Values)
            {
                // Check if the embedded resource is a collection of resources and if so
                // apply the metadata to each contained resource.
                if (embeddedResource is IEnumerable resourceColl)
                {
                    foreach (IHalResource childResource in resourceColl.OfType<IHalResource>())
                    {
                        ApplyMetadataToResource(childResource, resourceContext);
                    }

                    continue;
                }

                // Add metadata to single embedded resource.
                if (embeddedResource is IHalResource halResource)
                {
                    ApplyMetadataToResource(halResource, resourceContext);
                }
            }
        }

        private ResourceContext CreateContext(HttpContext httpContext)
        {
            return new ResourceContext
            {
                RestModule = httpContext.GetService<IRestModule>(),
                MediaModule = httpContext.GetService<IResourceMediaModule>(),
                ApiMetadata = httpContext.GetService<IApiMetadataService>(),
                UrlHelper = httpContext.GetService<IUrlHelper>(),
                Logger = httpContext.GetService<ILogger<HalFormatter>>()
            };
        }
    }
}