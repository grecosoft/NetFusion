using NetFusion.Common.Base.Validation;

namespace NetFusion.Integration.Redis.Plugin.Settings
{
    /// <summary>
    /// Contains settings for a specific Redis connection.
    /// </summary>
    public class DbConnection : IValidatableType
    {
        /// <summary>
        /// The name of the database connection.
        /// </summary>
        public string Name { get; internal set; } = string.Empty;
        
        /// <summary>
        /// Used specify a number associated with the connection allowing the
        /// Redis server to be partitioned into databases.
        /// </summary>
        public int? DefaultDatabaseId { get; set; }

        /// <summary>
        /// The number of times to repeat the initial connect cycle if no servers respond promptly
        /// </summary>
        public int ConnectRetry { get; set; } = 5;

        /// <summary>
        /// Specifies the time in milliseconds that should be allowed for connection
        /// (defaults to 5 seconds unless SyncTimeout is higher).
        /// </summary>
        public int ConnectTimeout { get; set; } = 5000;

        /// <summary>
        /// Specifies the time in seconds at which connections should be pinged to ensure validity
        /// </summary>
        public int KeepAlive { get; set; } = 10;
        
        /// <summary>
        /// The password to use to authenticate with the server
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// The endpoints used to building the connection.
        /// </summary>
        public IList<DbEndPoint> EndPoints { get; set; } = new List<DbEndPoint>();

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(string.IsNullOrWhiteSpace(Name), "Connection Name not specified.");
            validator.Verify(ConnectRetry > 0, "Connection Retry must be greater than zero.");
            validator.Verify(ConnectTimeout > 0, "Connection Timeout must be greater than zero.");
            validator.Verify(KeepAlive > 0, "Connection Keep-Alive must be greater than zero.");
            validator.AddChildren(EndPoints);
        }
    }
}