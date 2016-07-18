using System.Collections.Generic;

namespace NetFusion.Logging
{
    public class HostLog
    {
        // For De-Serialization.
        public HostLog()
        {

        }

        // Used by an external client that is posting its composite log to the server.
        public HostLog(string hostName, string hostPluginId, IDictionary<string, object> log)
        {
            this.HostName = hostName;
            this.HostPluginId = hostPluginId;
            this.Log = log;
        }

        // Used by the web server hosting a composite application.
        public HostLog(CompositeInfo compositeApp, IDictionary<string, object> log)
        {
            this.CompositeApp = compositeApp;
            this.Log = log;
            this.HostName = this.CompositeApp.AppHostPlugin.Plugin.Manifest.Name;
            this.HostPluginId = this.CompositeApp.AppHostPlugin.Plugin.Manifest.PluginId;
        }

        public string HostName { get; set; }
        public string HostPluginId { get; set; }

        public CompositeInfo CompositeApp { get; }
        public IDictionary<string, object> Log { get; set; }
    }
}
