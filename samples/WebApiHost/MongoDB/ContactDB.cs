using NetFusion.MongoDB.Configs;
using NetFusion.Settings;

namespace WebApi.MongoDB
{
    /// <summary>
    /// Defines the settings associated with a specific database.
    /// </summary>
    [ConfigurationSection("customers:contactDb")]
    public class ContactDB : MongoSettings
    {
    }
}
