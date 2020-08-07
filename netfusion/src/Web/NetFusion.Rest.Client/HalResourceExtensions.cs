using System;
using System.Collections.Generic;
using System.Text.Json;
using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Client
{
    /// <summary>
    /// Extension methods used by a .NET WebApi client used to obtain links
    /// and embedded resources and models.
    /// </summary>
    public static class HalResourceExtensions
    {
        static HalResourceExtensions()
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
        public static bool HasLink(this HalResource resource, string named)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of link not specified.", nameof(named));

            return resource.Links != null && resource.Links.ContainsKey(named);
        }
        
        /// <summary>
        /// Returns a link identified by name associated with resource.
        /// </summary>
        /// <param name="resource">The resource with associated links.</param>
        /// <param name="named">The name identifying the link.</param>
        /// <returns>The link if found.  Otherwise an exception is raised.</returns>
        public static Link GetLink(this HalResource resource, string named)
        {
            if (TryGetLink(resource, named, out Link link))
            {
                return link;
            }
            
            throw new InvalidOperationException($"Link named: {named} does not exists.");
        }

        /// <summary>
        /// Attempts to find a resource's link by name.
        /// </summary>
        /// <param name="resource">The resource with associated links.</param>
        /// <param name="named">The name identifying the link.</param>
        /// <param name="link">Will contain reference to link if found.</param>
        /// <returns>True if the link is found.  Otherwise False.</returns>
        public static bool TryGetLink(this HalResource resource, string named, out Link link)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named)) 
                throw new ArgumentException("Link name not specified.", nameof(named));

            link = null;
            return resource.Links != null && resource.Links.TryGetValue(named, out link);
        }
        
        /// <summary>
        /// Determines if the resource contains a named embedded resource/model.
        /// </summary>
        /// <param name="resource">The resource with embedded items.</param>
        /// <param name="named">The name identifying the embedded resource/model.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        public static bool HasEmbedded(this HalResource resource, string named)
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
        /// <param name="named">The name identifying the embedded model.</param>
        /// <typeparam name="TChildModel">The type of the embedded model.</typeparam>
        /// <returns>Reference to the deserialized model or an exception if not present.</returns>
        public static TChildModel GetEmbeddedModel<TChildModel>(this HalResource resource, string named)
            where TChildModel: class
        {
            if (TryGetEmbeddedModel(resource, named, out TChildModel model))
            {
                return model;
            }
            
            throw new InvalidOperationException(
                $"Embedded model named: {named} of parent resource does not exist.");
        }
        
        /// <summary>
        /// Returns an embedded model if present.
        /// </summary>
        /// <param name="resource">The parent resource containing the embedded model.</param>
        /// <param name="named">The name identifying the embedded model.</param>
        /// <param name="model">Reference to the embedded model if found.</param>
        /// <typeparam name="TChildModel">The type of the embedded model.</typeparam>
        /// <returns>True if the embedded model was found.  Otherwise, False.</returns>
        public static bool TryGetEmbeddedModel<TChildModel>(this HalResource resource, string named, 
            out TChildModel model)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded model not specified.", nameof(named));

            model = null;
            if (! resource.HasEmbedded(named))
            {
                return false;
            }
            
            // Check if the embedded resource has been deserialized from the base JsonElement
            // representation and return it.
            if (resource.Embedded[named] is TChildModel embeddedItem)
            {
                model = embeddedItem;
                return true;
            }

            // Deserialize the embedded JsonElement into a type object instance.
            if (resource.Embedded[named] is JsonElement embeddedJson)
            {
                model = JsonSerializer.Deserialize<TChildModel>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = model; // Override the JsonElement reference.
                return true;
            }

            if (model == null)
            {
                throw new InvalidCastException("The named embedded model: {named} does not contain a JsonElement.");   
            }

            return false;
        }

        /// <summary>
        /// Returns an instance of an embedded resource.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource's model.</typeparam>
        /// <param name="resource">The parent resource with embedded resource.</param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated embedded resource or an exception if not found.</returns>
        public static HalResource<TChildModel> GetEmbeddedResource<TChildModel>(this HalResource resource, string named)
            where TChildModel : class
        {
            if (TryGetEmbeddedResource<TChildModel>(resource, named, out var embeddedResource))
            {
                return embeddedResource;
            }
            
            throw new InvalidOperationException(
                $"Embedded resource named: {named} of parent resource does not exist.");
        }

        /// <summary>
        /// Returns an instance of an embedded resource.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource's model.</typeparam>
        /// <param name="resource">The parent resource with embedded resource.</param>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <param name="embeddedResource">Reference to the embedded resource if found.</param>
        /// <returns>True if the embedded resource if found.  Otherwise False.</returns>
        public static bool TryGetEmbeddedResource<TChildModel>(this HalResource resource, 
            string named, out HalResource<TChildModel> embeddedResource)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not specified.", nameof(named));

            embeddedResource = null;
            if (! resource.Embedded.ContainsKey(named))
            {
                return false;
            }
            
            // Check if the embedded resource has been deserialized from the base JsonElement representation and return it.
            if (resource.Embedded[named] is HalResource<TChildModel> embeddedItem)
            {
                embeddedResource = embeddedItem;
                return true;
            }

            // Deserialize the embedded JsonElement into a type object instance.
            if (resource.Embedded[named] is JsonElement embeddedJson)
            {
                embeddedResource = JsonSerializer.Deserialize<HalResource<TChildModel>>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = embeddedResource; // Override the JsonElement reference.
                return true;
            }

            if (embeddedResource == null)
            {
                throw new InvalidCastException("The named embedded model: {named} does not contain a JsonElement.");   
            }

            return false;
        }
        
        /// <summary>
        /// Returns an instance of an embedded resource collection.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource model.</typeparam>
        /// <param name="resource">The parent resource with embedded resources.</param>
        /// <param name="named">The name identifying the embedded resource collection.</param>
        /// <returns>List of embedded collection of resources if found.  Otherwise,
        /// and exception is raised.</returns>
        public static IEnumerable<HalResource<TChildModel>> GetEmbeddedResources<TChildModel>(
            this HalResource resource, string named)
            where TChildModel : class
        {
            if (TryGetEmbeddedResources<TChildModel>(resource, named, out var embeddedResources))
            {
                return embeddedResources;
            }
            
            throw new InvalidOperationException(
                $"Embedded resource collection named: {named} of parent resource does not exist.");
        }

        /// <summary>
        /// Returns an instance of an embedded resource collection.
        /// </summary>
        /// <typeparam name="TChildModel">The type of the embedded resource model.</typeparam>
        /// <param name="resource">The parent resource with embedded resources.</param>
        /// <param name="named">The name identifying the embedded resource collection.</param>
        /// <param name="embeddedResources">Reference to the embedded resources if found.</param>
        /// <returns>True if the list of embedded resources are found.  Otherwise, False.</returns>
        public static bool TryGetEmbeddedResources<TChildModel>(this HalResource resource, string named,
            out IEnumerable<HalResource<TChildModel>> embeddedResources)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource collection not provided.", nameof(named));

            embeddedResources = null;
            if (! resource.HasEmbedded(named))
            {
                return false;
            }

            if (resource.Embedded[named] is List<HalResource<TChildModel>> embeddedItems)
            {
                embeddedResources = embeddedItems;
                return true;
            }

            if (resource.Embedded[named] is JsonElement embeddedJson && embeddedJson.ValueKind == JsonValueKind.Array)
            {
                embeddedResources = JsonSerializer.Deserialize<List<HalResource<TChildModel>>>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = embeddedResources; // Override the JsonElement reference.
                return true;
            }

            if (embeddedResources == null)
            {
                throw new InvalidCastException(
                    $"The named embedded collection: {named} does not contain a JsonElement of type array.");
            }

            return false;
        }

        /// <summary>
        /// Returns an instance of an embedded collection of models.
        /// </summary>
        /// <param name="resource">The parent resource with embedded models.</param>
        /// <param name="named">The name identifying the embedded models.</param>
        /// <typeparam name="TChildModel">The type of the embedded model.</typeparam>
        /// <returns>Instance of the embedded collection of models.  If not found,
        /// and exceptions is raised.</returns>
        public static IEnumerable<TChildModel> GetEmbeddedModels<TChildModel>(this HalResource resource, string named)
            where TChildModel : class
        {
            if (TryGetEmbeddedModels<TChildModel>(resource, named, out var embeddedModels))
            {
                return embeddedModels;
            }
            
            throw new InvalidOperationException(
                $"Embedded model collection named: {named} of parent resource does not exist.");
        }

        /// <summary>
        /// Returns an instance of an embedded collection of models.
        /// </summary>
        /// <param name="resource">The parent resource with embedded models.</param>
        /// <param name="named">The name identifying the embedded models.</param>
        /// <param name="embeddedModels">Reference to the embedded models if found.</param>
        /// <typeparam name="TChildModel">The type of the embedded model.</typeparam>
        /// <returns>True if the embedded collection of models is found.  Otherwise, False.</returns>
        public static bool TryGetEmbeddedModels<TChildModel>(this HalResource resource, 
            string named, out IEnumerable<TChildModel> embeddedModels)
            where TChildModel: class
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded model collection not provided.", nameof(named));

            embeddedModels = null;
            if (! resource.HasEmbedded(named))
            {
                return false;
            }

            if (resource.Embedded[named] is List<TChildModel> embeddedItems)
            {
                embeddedModels = embeddedItems;
                return true;
            }

            if (resource.Embedded[named] is JsonElement embeddedJson && embeddedJson.ValueKind == JsonValueKind.Array)
            {
                embeddedModels = JsonSerializer.Deserialize<List<TChildModel>>(
                    embeddedJson.GetRawText(), 
                    DefaultOptions);
                
                resource.Embedded[named] = embeddedModels; // Override the JsonElement reference.
                return true;
            }

            if (embeddedModels == null)
            {
                throw new InvalidCastException(
                    $"The named embedded collection: {named} does not contain a JsonElement of type array.");
            }
            
            return false;
        }
    }
}