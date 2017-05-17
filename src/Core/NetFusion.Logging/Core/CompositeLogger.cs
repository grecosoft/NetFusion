using NetFusion.Common;
using NetFusion.Logging.Configs;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace NetFusion.Logging.Core
{
    /// <summary>
    /// Logger component that will publish an application's log to a configured
    /// endpoint and route.  The endpoint will usually be a WebApi application
    /// with a user-interface that can be used to view the logs associated with
    /// hosts that not having a user-interface (i.e. Windows Service).
    /// </summary>
    public class CompositeLogger : ICompositeLogger
    {
        private readonly CompositeLogSettings _logSettings;

        public CompositeLogger(CompositeLogSettings logSettings)
        {
            Check.NotNull(logSettings, nameof(logSettings));

            _logSettings = logSettings;
        }

        public void Log(HostLog hostLog)
        {
            if (! _logSettings.SendLog) return;

            var jsonLog = JsonConvert.SerializeObject(hostLog);
            var client = new HttpClient { BaseAddress = new Uri(_logSettings.Endpoint) };
            var content = new StringContent(jsonLog, Encoding.UTF8, "application/json");

           var result = client.PostAsync(_logSettings.LogRoute, content).Result;
        }
    }
}
