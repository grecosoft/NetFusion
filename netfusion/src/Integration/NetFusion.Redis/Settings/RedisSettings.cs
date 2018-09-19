using System.Collections.Generic;
using NetFusion.Base.Validation;
using NetFusion.Settings;

namespace NetFusion.Redis.Settings
{
    /// <summary>
    /// Settings used to connect to a named set of Redis servers.
    ///
    /// https://github.com/grecosoft/NetFusion/wiki/core.settings.overview#settings---overview
    /// </summary>
    [ConfigurationSection("netfusion:redis")]
    public class RedisSettings : IAppSettings,
        IValidatableType
    {
        public IList<DbConnection> Connections { get; set; } = new List<DbConnection>();
        
        public void Validate(IObjectValidator validator)
        {
            validator.AddChildren(Connections);
        }
    }
}