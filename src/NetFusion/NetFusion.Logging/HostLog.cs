using System.Collections.Generic;

namespace NetFusion.Logging
{
    public class HostLog
    {
        /// <summary>
        /// For De-Serialization.
        /// </summary>
        public HostLog()
        {

        }

        /// <summary>
        /// Used by an external client that is posting its composite log to the server.
        /// </summary>
        /// <param name="hostName">The name of the application host submitting the log.</param>
        /// <param name="hostPluginId">The identity value of the host submitting the log.</param>
        /// <param name="log">The host log information to submit.</param>
        public HostLog(string hostName, string hostPluginId, IDictionary<string, object> log)
        {
            this.HostName = hostName;
            this.HostPluginId = hostPluginId;
            this.Log = log;
        }

        /// <summary>
        /// Used by the web server hosting a composite application and the endpoint to which
        /// other client hosts can publish.  This constructor creates the instance that is
        /// returned to the user-interface for rendering.
        /// </summary>
        /// <param name="compositeApp"></param>
        /// <param name="log"></param>
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
