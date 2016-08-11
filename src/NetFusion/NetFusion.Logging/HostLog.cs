using NetFusion.Common;
using System.Collections.Generic;

namespace NetFusion.Logging
{
    public class HostLog
    {
        /// <summary>
        /// The name of the host application.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The plug-in id of the host application.
        /// </summary>
        public string HostPluginId { get; set; }

        /// <summary>
        /// High level summary of the plug-ins contained within the composite application.
        /// </summary>
        public CompositeInfo CompositeApp { get; }

        /// <summary>
        /// The detailed log of the host.
        /// </summary>
        public IDictionary<string, object> Log { get; set; }

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
            Check.NotNullOrWhiteSpace(hostName, nameof(hostName));
            Check.NotNullOrWhiteSpace(hostPluginId, nameof(hostPluginId));
            Check.NotNull(log, nameof(log));

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
            Check.NotNull(compositeApp, nameof(compositeApp));
            Check.NotNull(log, nameof(log));

            this.CompositeApp = compositeApp;
            this.Log = log;
            this.HostName = this.CompositeApp.AppHostPlugin.Plugin.Manifest.Name;
            this.HostPluginId = this.CompositeApp.AppHostPlugin.Plugin.Manifest.PluginId;
        }
    }
}
