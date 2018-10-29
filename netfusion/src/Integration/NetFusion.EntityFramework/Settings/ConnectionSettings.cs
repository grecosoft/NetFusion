using System.Collections.Generic;
using NetFusion.Settings;

namespace NetFusion.EntityFramework.Settings
{
    /// <summary>
    /// Application configuration class used to specify connections
    /// for EntityFramework database context classes.
    /// </summary>
    [ConfigurationSection("netfusion:entityFramework")]
    public class ConnectionSettings : IAppSettings
    {
        /// <summary>
        /// List of settings used by a specific derived EntityDbContext.
        /// </summary>
        public ICollection<DbContextSettings> Contexts { get; set; }

        public ConnectionSettings()
        {
            Contexts = new List<DbContextSettings>();
        }
    }
}