using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Meta;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Web.Mvc.Metadata;

namespace NetFusion.Rest.Server.Mappings
{
    /// <summary>
    /// Context class containing information for the resource and services
    /// used by a provider when mapping resources.
    /// </summary>
    public class ResourceContext
    {
        // Instance of a resource being returned.
        public IResource Resource { get; set; }

        // Optional reference to the state associated with the resource.
        // Based on the media-type, this value may not be set.
        public object Model { get; set; }

        // The metadata associated with the source type.
        public IResourceMeta Meta { get; set; }

        // Services that can be utilized when adding the metadata
        // to the resource instance.
        public IResourceMediaModule MediaModule { get; set; }
        public IApiMetadataService ApiMetadata { get; set; }
        public IUrlHelper UrlHelper { get; set; }
        public ILogger Logger { get; set; }
    }
}
