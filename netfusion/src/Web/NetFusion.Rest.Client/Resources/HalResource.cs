using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetFusion.Rest.Client.Resources
{
    /// <summary>
    /// Base resource class from which application resources are derived.  
    /// Also allows the consumer of the resource to obtain any embedded 
    /// named resources.
    /// </summary>
    public class HalResource
    {
        /// <summary>
        /// Embedded related named resources.
        /// </summary>
        [JsonPropertyName("_embedded")]
		public Dictionary<string, object> Embedded { get; set; }

        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        [JsonPropertyName("_links")]
        public Dictionary<string, Link> Links { get; set; }
        
        /// <summary>
        /// Determines if the resource contains a named link.
        /// </summary>
        /// <param name="named">The name identifying the link.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        public bool HasLink(string named)
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of link not provided.", nameof(named));

            return Links != null && Links.ContainsKey(named);
        }

        /// <summary>
        /// Returns a link identified by name associated with resource.
        /// </summary>
        /// <param name="named">The name identifying the link.</param>
        /// <returns>The link if found.  Otherwise an exception is raised.</returns>
        public Link GetLink(string named)
        {
            if (! HasLink(named))
            {
                throw new InvalidOperationException(
                    $"Link named: {named} for resource type: {GetType().FullName} does not exists.");
            }

            return Links[named];
        }
        
        /// <summary>
        /// Determines if the resources contains a named embedded resource.
        /// </summary>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        public bool HasEmbedded(string named)
        {
			if (string.IsNullOrWhiteSpace(named))
				throw new ArgumentException("Name of embedded resource not provided.", nameof(named));
            
            return Embedded != null && Embedded.ContainsKey(named);
        }

        /// <summary>
        /// Returns an instance of an embedded resource.
        /// </summary>
        /// <typeparam name="TModel">The type of the embedded resource.</typeparam>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated nested type.</returns>
        public HalResource<TModel> GetEmbedded<TModel>(string named)
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));

            if (! HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource named: {named} of parent resource type: {GetType().FullName} does not exist.");
            }
            
            // Check if the embedded resource has been deserialized from the base JObject representation and return it.
            if (Embedded[named] is HalResource<TModel> embededItem)
            {
                return embededItem;
            }

            // Deserialize the embedded JObject into a type object instance.
            if (Embedded[named] is JsonElement embeddedJObj)
            {
                embededItem = JsonSerializer.Deserialize<HalResource<TModel>>(embeddedJObj.GetRawText(), 
                    new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                Embedded[named] = embededItem; // Override the JObject reference.
                return embededItem;
            }

            throw new InvalidCastException(
                $"The named embedded value of: {named} does not contain a JObject.");
        }

        /// <summary>
        /// Returns an instance of an embedded collection resource.
        /// </summary>
        /// <typeparam name="TModel">The type of the embedded resource array item.</typeparam>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated nested array type.</returns>
        public IEnumerable<HalResource<TModel>> GetEmbeddedCollection<TModel>(string named)
      
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));

            if (! HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource array named: {named} of parent resource type: {GetType().FullName} does not exist.");
            }

            if (Embedded[named] is List<HalResource<TModel>> embededItem)
            {
                return embededItem;
            }

            if (Embedded[named] is JsonElement embedJArray)
            {
                embededItem = JsonSerializer.Deserialize<List<HalResource<TModel>>>(embedJArray.GetRawText(), 
                    new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                Embedded[named] = embededItem;
                return embededItem;
            }

            throw new InvalidCastException(
                $"The named embedded array value of: {named} does not contain a JArray.");
        }
    }
    
    public class HalResource<TModel> : HalResource
    {
        public TModel Model { get; set; }
    }
}
