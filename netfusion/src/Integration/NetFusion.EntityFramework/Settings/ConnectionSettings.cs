using System.Collections.Generic;
using NetFusion.Settings;

namespace NetFusion.EntityFramework.Settings
{
    using NetFusion.Base.Validation;
    using NetFusion.Common.Extensions.Collections;

    /// <summary>
    /// Application configuration class used to specify connections
    /// for EntityFramework database context classes.
    /// </summary>
    [ConfigurationSection("netfusion:entityFramework")]
    public class ConnectionSettings : IAppSettings,
        IValidatableType
    {
        /// <summary>
        /// List of settings used by derived EntityDbContext types.
        /// </summary>
        public ICollection<DbContextSettings> Contexts { get; set; }

        public ConnectionSettings()
        {
            Contexts = new List<DbContextSettings>();
        }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(
                Contexts.WhereDuplicated(c => c.ContextName).Empty(), 
                "Context names must be unique.");
        }
    }
}