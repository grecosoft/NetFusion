using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NetFusion.Rest.Client.Resources
{
    public static class HalResourceExtensions
    {
        /// <summary>
        /// Determines if the resource contains a named link.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="named">The name identifying the link.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        public static bool HasLink(this IHalResource resource, string named)
        {    
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of link not provided.", nameof(named));

            return resource.Links != null && resource.Links.ContainsKey(named);
        }
        
        /// <summary>
        /// Returns a link identified by name associated with resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="named">The name identifying the link.</param>
        /// <returns>The link if found.  Otherwise an exception is raised.</returns>
        public static Link GetLink(this IHalResource resource, string named)
        {
            if (! resource.HasLink(named))
            {
                throw new InvalidOperationException(
                    $"Link named: {named} for resource type: {resource.GetType().FullName} does not exists.");
            }

            return resource.Links[named];
        }
        
        /// <summary>
        /// Determines if the resources contains a named embedded resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        public static bool HasEmbedded(this IHalResource resource, string named)
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));
            
            return resource.Embedded != null && resource.Embedded.ContainsKey(named);
        }
        
        /// <summary>
        /// Returns an instance of an embedded resource.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource.</typeparam>
        /// <param name="resource"></param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated nested type.</returns>
        public static HalResource<TChildModel> GetEmbedded<TChildModel>(this IHalResource resource, string named)
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));

            if (! resource.HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource named: {named} of parent resource type: {resource.GetType().FullName} does not exist.");
            }
            
            // Check if the embedded resource has been deserialized from the base JObject representation and return it.
            if (resource.Embedded[named] is HalResource<TChildModel> embededItem)
            {
                return embededItem;
            }

            // Deserialize the embedded JObject into a type object instance.
            if (resource.Embedded[named] is JsonElement embeddedJObj)
            {
                embededItem = JsonSerializer.Deserialize<HalResource<TChildModel>>(embeddedJObj.GetRawText(), 
                    new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                resource.Embedded[named] = embededItem; // Override the JObject reference.
                return embededItem;
            }

            throw new InvalidCastException(
                $"The named embedded value of: {named} does not contain a JObject.");
        }
        
        /// <summary>
        /// Returns an instance of an embedded collection resource.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource array item.</typeparam>
        /// <param name="resource"></param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated nested array type.</returns>
        public static IEnumerable<HalResource<TChildModel>> GetEmbeddedCollection<TChildModel>(this IHalResource resource, string named)
      
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));

            if (! resource.HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource array named: {named} of parent resource type: {resource.GetType().FullName} does not exist.");
            }

            if (resource.Embedded[named] is List<HalResource<TChildModel>> embededItem)
            {
                return embededItem;
            }

            if (resource.Embedded[named] is JsonElement embedJArray)
            {
                embededItem = JsonSerializer.Deserialize<List<HalResource<TChildModel>>>(embedJArray.GetRawText(), 
                    new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
                resource.Embedded[named] = embededItem;
                return embededItem;
            }

            throw new InvalidCastException(
                $"The named embedded array value of: {named} does not contain a JArray.");
        }
    }
}