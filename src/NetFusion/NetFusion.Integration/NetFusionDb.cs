using NetFusion.MongoDB.Configs;

namespace NetFusion.Integration
{
    public class NetFusionDb : MongoSettings
    {
        public NetFusionDb()
        {
            this.DatabaseName = "NetFusion";
        }
    }
}
