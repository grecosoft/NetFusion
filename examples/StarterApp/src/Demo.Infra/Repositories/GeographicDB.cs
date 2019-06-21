using NetFusion.MongoDB.Settings;
 using NetFusion.Settings;

namespace Demo.Infra.Repositories
{
   [ConfigurationSection("netfusion:mongoDB:geographicDB")]
   public class GeographicDB : MongoSettings
   {

   }
}

