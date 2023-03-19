using NetFusion.Common.Base.Validation;
using NetFusion.Core.Settings;

namespace NetFusion.Integration.Redis.Plugin.Settings
{
    /// <summary>
    /// Settings used to connect to a named set of Redis servers.
    /// </summary>
    [ConfigurationSection("netfusion:redis")]
    public class RedisSettings : IAppSettings,
        IValidatableType
    {
        public IDictionary<string, DbConnection> Connections { get; set; } = new Dictionary<string, DbConnection>();

        /// <summary>
        /// The configuration represents a collection of items by keyed named.
        /// Updates the name on each item to that of the key specified within
        /// the configuration.
        /// </summary>
        public void SetNamedConfigurations()
        {
            foreach (var (name, conn) in Connections)
            {
                conn.Name = name;
            }
        }
        
        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Connections.Values);
        }
    }
}