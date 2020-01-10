using System;
using System.Collections.Generic;
using System.Text.Json;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Extension methods used by a .NET WebApi client used to obtain links
    /// and embedded resources and models
    /// </summary>
    public static class HalResourceClientExtensions
    {
        static HalResourceClientExtensions()
        {
            DefaultOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        
        private static JsonSerializerOptions DefaultOptions { get; } 

        /// <summary>
        /// Determines if the resource contains a named link.
        /// </summary>
        /// <param name="resource">The resource with associated links.</param>
        /// <param name="named">The name identifying the link.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        public static bool HasLink(this IHalResource resource, string named)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of link not provided.", nameof(named));

            return resource.Links != null && resource.Links.ContainsKey(named);
        }
        
        /// <summary>
        /// Returns a link identified by name associated with resource.
        /// </summary>
        /// <param name="resource">The resource with associated links.</param>
        /// <param name="named">The name identifying the link.</param>
        /// <returns>The link if found.  Otherwise an exception is raised.</returns>
        public static Link GetLink(this IHalResource resource, string named)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (! resource.HasLink(named))
            {
                throw new InvalidOperationException($"Link named: {named} does not exists.");
            }

            return resource.Links[named];
        }
        
        /// <summary>
        /// Determines if the resource contains a named embedded resource/model.
        /// </summary>
        /// <param name="resource">The resource with embedded resources.</param>
        /// <param name="named">The name identifying the embedded resource/model.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        public static bool HasEmbedded(this IHalResource resource, string named)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));
            
            return resource.Embedded != null && resource.Embedded.ContainsKey(named);
        }

        /// <summary>
        /// Returns an embedded resource model.
        /// </summary>
        /// <param name="resource">The parent resource containing the embedded model.</param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <typeparam name="TChildModel">The type of the embedded model.</typeparam>
        /// <returns>Reference to the deserialized model or an exception if not present.</returns>
        public static TChildModel GetEmbeddedModel<TChildModel>(this IHalResource resource, string named)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded model not provided.", nameof(named));

            if (! resource.HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded model named: {named} of parent resource does not exist.");
            }
            
            // Check if the embedded resource has been deserialized from the base JsonElement representation and return it.
            if (resource.Embedded[named] is TChildModel embeddedItem)
            {
                return embeddedItem;
            }

            // Deserialize the embedded JObject into a type object instance.
            if (resource.Embedded[named] is JsonElement embeddedJson)
            {
                embeddedItem = JsonSerializer.Deserialize<TChildModel>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = embeddedItem; // Override the JsonElement reference.
                return embeddedItem;
            }

            throw new InvalidCastException("The named embedded model: {named} does not contain a JsonElement.");   
        }
        
        /// <summary>
        /// Returns an instance of an embedded resource.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource's model.</typeparam>
        /// <param name="resource">The parent resource with embedded resources.</param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated embedded resource.</returns>
        public static HalResource<TChildModel> GetEmbeddedResource<TChildModel>(this IHalResource resource, string named)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));

            if (! resource.HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource named: {named} of parent resource does not exist.");
            }
            
            // Check if the embedded resource has been deserialized from the base JsonElement representation and return it.
            if (resource.Embedded[named] is HalResource<TChildModel> embeddedItem)
            {
                return embeddedItem;
            }

            // Deserialize the embedded JObject into a type object instance.
            if (resource.Embedded[named] is JsonElement embeddedJson)
            {
                embeddedItem = JsonSerializer.Deserialize<HalResource<TChildModel>>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = embeddedItem; // Override the JsonElement reference.
                return embeddedItem;
            }

            throw new InvalidCastException("The named embedded resource: {named} does not contain a JsonElement.");
        }
        
        /// <summary>
        /// Returns an instance of an embedded resource collection.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource model.</typeparam>
        /// <param name="resource">The parent resource with embedded resources.</param>
        /// <param name="named">The name identifying the embedded resource collection.</param>
        /// <returns>List of embedded collection of resources.</returns>
        public static IEnumerable<HalResource<TChildModel>> GetEmbeddedResources<TChildModel>(this IHalResource resource, string named)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource collection not provided.", nameof(named));

            if (! resource.HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource collection named: {named} of parent resource does not exist.");
            }

            if (resource.Embedded[named] is List<HalResource<TChildModel>> embeddedItem)
            {
                return embeddedItem;
            }

            if (resource.Embedded[named] is JsonElement embeddedJson && embeddedJson.ValueKind == JsonValueKind.Array)
            {
                embeddedItem = JsonSerializer.Deserialize<List<HalResource<TChildModel>>>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = embeddedItem; // Override the JsonElement reference.
                return embeddedItem;
            }

            throw new InvalidCastException(
                $"The named embedded collection: {named} does not contain a JsonElement of type array.");
        }
        
        /// <summary>
        /// Returns an instance of an embedded collection of models.
        /// </summary>
        /// <param name="resource">The parent resource with embedded resources.</param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <typeparam name="TChildModel">The type of the embedded model.</typeparam>
        /// <returns>Instance of the embedded collection of models..</returns>
        public static IEnumerable<TChildModel> GetEmbeddedModels<TChildModel>(this IHalResource resource, string named)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded model collection not provided.", nameof(named));

            if (! resource.HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded model collection named: {named} of parent resource does not exist.");
            }

            if (resource.Embedded[named] is List<TChildModel> embeddedItem)
            {
                return embeddedItem;
            }

            if (resource.Embedded[named] is JsonElement embeddedJson && embeddedJson.ValueKind == JsonValueKind.Array)
            {
                embeddedItem = JsonSerializer.Deserialize<List<TChildModel>>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = embeddedItem; // Override the JsonElement reference.
                return embeddedItem;
            }

            throw new InvalidCastException(
                $"The named embedded collection: {named} does not contain a JsonElement of type array.");
        }
    }
}