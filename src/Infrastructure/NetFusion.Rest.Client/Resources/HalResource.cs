using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Client.Resources
{
    /// <summary>
    /// Base resource class from which application resources are derived.  
    /// Also allows the consumer of the resource to obtain any embedded 
    /// named resources.
    /// </summary>
    public abstract class HalResource
    {
        /// <summary>
        /// Embedded related named resources.
        /// </summary>
        [JsonProperty(PropertyName = "_embedded")]
		public Dictionary<string, object> Embedded { get; set; }

        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        [JsonProperty(PropertyName = "_links")]
        public Dictionary<string, Link> Links { get; set; }

        // When submitting resources back to the server for updating or 
        // other use-cases, the embedded resources should not be serialized.
        public bool ShouldSerializeEmbedded() => false;
        public bool ShouldSerializeLinks() => false;

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
        /// <typeparam name="TResource">The type of the embedded resource.</typeparam>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated nested type.</returns>
        public TResource GetEmbedded<TResource>(string named)
            where TResource : HalResource
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));

            if (!HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource named: {named} of parent resource type: {GetType().FullName} does not exist.");
            }

            // Note:  The server side Web REST/HAL implementation of the Embedded property returns a simple dictionary 
            // with the value based on a common interface.  This means that the exact type of the embedded resource is 
            // not known when the Embedded property is being deserialized on this side.  Since this is the case, 
            // the value for each item in the dictionary will be of type JObject.  The consuming developer will know
            // the embedded type structure and have defined a matching class.  This approach keeps the server and the
            // client code less complex by not having to define typed classes for the Embedded property and allows 
            // resources to be combined to create new resource types.

            // Check if the embedded resource has been deserialized from the base JObject representation and return it.
            if (Embedded[named] is TResource embededItem)
            {
                return embededItem;
            }

            // Deserialize the embedded JObject into a type object instance.
            if (Embedded[named] is JObject embededJObj)
            {
                embededItem = embededJObj.ToObject<TResource>();
                Embedded[named] = embededItem; // Override the JObject reference.
                return embededItem;
            }

            throw new InvalidCastException(
                $"The named embedded value of: {named} does not contain a JObject.");
        }

        /// <summary>
        /// Returns an instance of an embedded collection resource.
        /// </summary>
        /// <typeparam name="TResource">The type of the embedded resource array item.</typeparam>
        /// <param name="named">The name identifying the embedded resource.</param>
        /// <returns>Instance of the populated nested array type.</returns>
        public IEnumerable<TResource> GetEmbeddedCollection<TResource>(string named)
            where TResource : HalResource
        {
            if (string.IsNullOrWhiteSpace(named))
                throw new ArgumentException("Name of embedded resource not provided.", nameof(named));

            if (!HasEmbedded(named))
            {
                throw new InvalidOperationException(
                    $"Embedded resource array named: {named} of parent resource type: {GetType().FullName} does not exist.");
            }

            if (Embedded[named] is List<TResource> embededItem)
            {
                return embededItem;
            }

            if (Embedded[named] is JArray embededJArray)
            {
                embededItem = embededJArray.ToObject<List<TResource>>();
                Embedded[named] = embededItem;
                return embededItem;
            }

            throw new InvalidCastException(
                $"The named embedded array value of: {named} does not contain a JArray.");
        }
    }
}
