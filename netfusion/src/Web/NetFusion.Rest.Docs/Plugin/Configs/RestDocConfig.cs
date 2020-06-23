using System;
using System.Collections.Generic;
using System.Text.Json;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.AttributeDescriptions;
using NetFusion.Rest.Docs.Attributes;
using NetFusion.Rest.Docs.Core.Description;
using NetFusion.Rest.Docs.XmlDescriptions;

namespace NetFusion.Rest.Docs.Plugin.Configs
{
    public class RestDocConfig : IPluginConfig
    {
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
            
            AddDocDescription<EmbeddedActionAttributes>();
            AddDocDescription<XmlActionComments>();
            AddDocDescription<XmlParamComments>();
            AddDocDescription<XmlResponseComments>();

            DescriptionTypes = _descriptionTypes.AsReadOnly();
        }

        public void UseEndpoint(string endpoint)
        {
            EndpointUrl = endpoint;
        }

        public void UseSerializationOptions(JsonSerializerOptions options)
        {
            SerializerOptions = options;
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