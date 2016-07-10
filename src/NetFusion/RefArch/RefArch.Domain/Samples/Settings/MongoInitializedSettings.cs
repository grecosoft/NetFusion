using NetFusion.Settings;

namespace RefArch.Domain.Samples.Settings
{
    public class MongoInitializedSettings : AppSettings
    {
        public MongoInitializedSettings()
        {
            this.ApplicationId = "576b2a97000a7ed1176f1771";
            this.Environment = EnvironmentTypes.Development;
            this.IsInitializationRequired = false;
        }

        public int Value1 { get; set; } = 100;
        public int Value2 { get; set; } = 200;
    }
}