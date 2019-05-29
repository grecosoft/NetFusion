namespace Service.Infra.Databases
{
    using NetFusion.MongoDB.Settings;
    using NetFusion.Settings;

    [ConfigurationSection("databases:contacts")]
    public class ContactDb : MongoSettings
    {
        
    }
}