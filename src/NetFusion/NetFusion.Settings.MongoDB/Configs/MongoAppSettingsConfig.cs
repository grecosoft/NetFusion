    using NetFusion.Bootstrap.Container;

namespace NetFusion.Settings.Mongo.Configs
{
    /// <summary>
    /// Class containing the settings used to load applications settings
    /// from MongoDB.
    /// </summary>
    public class MongoAppSettingsConfig : IContainerConfig
    {
        public string MongoUrl { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "NetFusion";
        public string CollectionName { get; set; } = "NetFusion.AppSettings";
    }
}
