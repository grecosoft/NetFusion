using NetFusion.Settings;
using System.Collections.Generic;

namespace NetFusion.EntityFramework.Configs
{
    /// <summary>
    /// Provided by the application host to specify the database connections that
    /// should be used for all the Entity Framework contexts.
    /// </summary>
    [ConfigurationSection("netfusion:plugins:entityFramework")]
    public class ContextSettings : AppSettings
    {
        /// <summary>
        /// List of context database context connection strings.
        /// </summary>
        public IEnumerable<ContextConnection> Connections { get; set; }
    }
}
