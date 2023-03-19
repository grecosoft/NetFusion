using NetFusion.Common.Base.Validation;

namespace NetFusion.Integration.Redis.Plugin.Settings
{
    /// <summary>
    /// Settings for a specific Redis database instance.
    /// </summary>
    public class DbEndPoint : IValidatableType
    {
        /// <summary>
        /// The host of the Redis server.
        /// </summary>
        public string Host { get; set; } = string.Empty;
        
        /// <summary>
        /// The port on which the server is listening for requests.
        /// </summary>
        public int Port { get; set; }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(! string.IsNullOrWhiteSpace(Host), "Endpoint Host not specified.");
            validator.Verify(Port > 0, "Endpoint Port must be greater than zero.");
        }
    }
}