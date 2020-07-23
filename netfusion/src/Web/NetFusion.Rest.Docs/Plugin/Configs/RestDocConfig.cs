using System;
using System.Collections.Generic;
using System.Text.Json;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Core.Descriptions;
using NetFusion.Rest.Docs.Xml.Descriptions;

namespace NetFusion.Rest.Docs.Plugin.Configs
{
    /// <summary>
    /// Plugin configuration used to alter the default configuration.
    /// </summary>
    public class RestDocConfig : IPluginConfig
    {
        /// <summary>
        /// The directory containing any needed documentation source files.
        /// </summary>
        public string DescriptionDirectory { get; private set; } = AppContext.BaseDirectory;

        /// <summary>
        /// The end point that can be called to request documentation for a WebApi action method.
        /// If not specified, the endpoint will be: /api/net-fusion/rest
        /// </summary>
        public string EndpointUrl { get; private set; }

        /// <summary>
        /// The JSON serialization options used to serialize the action
        /// documentation model.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; private set; }
        
        /// <summary>
        /// The description registered types used to lookup documentation
        /// to describe parts of a WebApi method. 
        /// </summary>
        public IReadOnlyCollection<Type> DescriptionTypes { get; }

        private readonly List<Type> _descriptionTypes = new List<Type>();

        public RestDocConfig()
        {
            ConfigureDefaultSettings();
            AddDefaultDescriptions();
            
            DescriptionTypes = _descriptionTypes.AsReadOnly();
        }

        private void ConfigureDefaultSettings()
        {
            EndpointUrl = "/api/net-fusion/rest";
            
            SerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // These are the default registered classes responsible for applying 
        // documentation associated with a controller's action metadata.
        private void AddDefaultDescriptions()
        {
            AddDocDescription<XmlActionComments>();
            AddDocDescription<XmlParameterComments>();
            AddDocDescription<XmlResponseComments>();
            AddDocDescription<XmlHalEmbeddedComments>();
            AddDocDescription<XmlHalRelationComments>();
        }

        /// <summary>
        /// Used to specify the directory containing additional documentation
        /// files used to describe WebApi action methods.
        /// </summary>
        /// <param name="directory">The directory to use.  If not specified,
        /// the application's context base directory is used.</param>
        /// <returns>The configuration.</returns>
        public RestDocConfig SetDescriptionDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Directory must be specified", nameof(directory));
            }

            DescriptionDirectory = directory;
            return this;
        }

        /// <summary>
        /// The end point that can be called to request documentation for a WebApi action method.
        /// </summary>
        /// <param name="endpoint">The relative URL.</param>
        /// <returns>The configuration.</returns>
        public RestDocConfig UseEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint must be specified.", nameof(endpoint));
            }

            EndpointUrl = endpoint;
            return this;
        }

        /// <summary>
        /// The JSON serialization options used to serialize the action documentation model.
        /// </summary>
        /// <param name="options">JSON serialization options.</param>
        /// <returns>The configuration.</returns>
        public RestDocConfig UseSerializationOptions(JsonSerializerOptions options)
        {
            SerializerOptions = options ?? throw new ArgumentNullException(nameof(options));
            return this;
        }

        /// <summary>
        /// Adds a documentation description class invoked to apply documentation
        /// to the returned Api Action documentation model.  These classes are
        /// invoked in the order in which they are registered.
        /// </summary>
        /// <paramtype name="T">The type implementing the lookup of additional
        /// documentation.</paramtype>
        /// <returns>The configuration.</returns>
        public RestDocConfig AddDocDescription<T>()
            where T : IDocDescription
        {
            if (! _descriptionTypes.Contains(typeof(T)))
            {
                _descriptionTypes.Add(typeof(T));
            }

            return this;
        }

        /// <summary>
        /// Clears all WebApi description classes.
        /// </summary>
        /// <returns>The configuration.</returns>
        public RestDocConfig ClearDescriptions()
        {
            _descriptionTypes.Clear();
            return this;
        }
    }
}