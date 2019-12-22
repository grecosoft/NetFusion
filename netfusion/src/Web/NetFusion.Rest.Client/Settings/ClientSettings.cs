using System;
using System.Text.Json;

namespace NetFusion.Rest.Client.Settings
{
    /// <summary>
    /// Contains settings that are associated with a given URL and
    /// used to configure the underlying HttpClient.
    /// </summary>
    public class ClientSettings
    {
        public ClientSettings()
        {
            SerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        
        public TimeSpan? Timeout { get; set; }
        public int? ConnectionLeaseTimeout { get; set; }
        public int? ConnectionLimit { get; set; }
        public int? DnsRefreshTimeout { get; set; }
        public JsonSerializerOptions SerializerOptions { get; set; }
    }
}