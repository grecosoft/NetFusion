using NetFusion.Settings;

namespace NetFusion.Logging.Configs
{
    public class CompositeLogSettings : AppSettings
    {
        public string Endpoint { get; set; }
        public string LogRoute { get; set; }
        public bool SendLog { get; set; } = false;
    }
}
