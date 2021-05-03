using System.Collections.Generic;
using NetFusion.Base.Validation;
using NetFusion.Settings;

namespace NetFusion.Redis.Settings
{
    /// <summary>
    /// Settings used to connect to a named set of Redis servers.
    /// </summary>
    [ConfigurationSection("netfusion:redis")]
    public class RedisSettings : IAppSettings,
        IValidatableType
    {
        public IDictionary<string, DbConnection> Connections { get; set; } = new Dictionary<string, DbConnection>();
        
        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Connections.Values);
        }
    }
}