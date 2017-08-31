using System.Collections.Generic;

namespace NetFusion.Rest.Config
{
    /// <summary>
    /// Contains settings for a given external API that can be consumed by the host application.
    /// </summary>
    public class ClientSettings
    {
        /// <summary>
        /// String key value used to reference the client within code.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// The base address associated with the client.  All made requests
        /// using the client have the base address appended to the URL of the request.
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// The URL to call that will return a resource containing links for all
        /// entry points into the API.  The client uses these links to load initial
        /// resources to which navigation to related resources can be transversed.
        /// </summary>
        public string EntryPointUrl { get; set; }

        /// <summary>
        /// Indicates when creating the client it should be configured to request REST-HAL
        /// resources.
        /// </summary>
        public bool UseHalDefaults { get; set; }

        public string AcceptType { get; set; }

        public string ContentType { get; set; }

        /// <summary>
        /// Default set of headers to be used for each request made by the client.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The time in milliseconds after which the request should timeout.
        /// </summary>
        public int TimeoutMs { get; set; }  
    }
}
