using System;
using System.Collections.Generic;
using System.Text.Json;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.Metadata;
using NetFusion.Rest.Docs.XmlDescriptions;

namespace NetFusion.Rest.Docs.Plugin.Configs
{
    public class RestDocConfig : IPluginConfig
    {
        public string DescriptionDirectory { get; set; } = AppContext.BaseDirectory;
        public string EndpointUrl { get; private set; }
        public JsonSerializerOptions SerializerOptions { get; private set; }
        private readonly List<Type> _descriptionTypes = new List<Type>();

        public RestDocConfig()
        {
            EndpointUrl = "/api/net-fusion/rest";
            
            SerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            AddDocDescription<EmbeddedResourceMeta>();
            AddDocDescription<XmlActionComments>();
            AddDocDescription<XmlParamComments>();
            AddDocDescription<XmlResponseComments>();
            AddDocDescription<XmlEmbeddedComments>();

            DescriptionTypes = _descriptionTypes.AsReadOnly();
        }

        public RestDocConfig SetDescriptionDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Directory must be specified", nameof(directory));
            }

            DescriptionDirectory = directory;
            return this;
        }

        public void UseEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint must be specified.", nameof(endpoint));
            }

            EndpointUrl = endpoint;
        }

        public void UseSerializationOptions(JsonSerializerOptions options)
        {
            SerializerOptions = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void AddDocDescription<T>()
            where T : IDocDescription
        {
            if (_descriptionTypes.Contains(typeof(T))) return;
            
            _descriptionTypes.Add(typeof(T));
        }
        
        public IReadOnlyCollection<Type> DescriptionTypes { get; }
    }
}