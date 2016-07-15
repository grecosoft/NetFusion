using NetFusion.Logging.Configs;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace NetFusion.Logging.Core
{
    public class CompositeLogger : ICompositeLogger
    {
        private readonly CompositeLogSettings _logSettings;

        public CompositeLogger(CompositeLogSettings logSettings)
        {
            _logSettings = logSettings;
        }

        public void Log(HostLog hostLog)
        {
            if (!_logSettings.SendLog) return;

            // Note RestSharp is not being used so the exact same serialization
            // is being used as the server.  This keeps the JSON serialization
            // structure the exact same for when viewing.
            var jsonLog = JsonConvert.SerializeObject(hostLog);
            var client = new HttpClient { BaseAddress = new Uri(_logSettings.Endpoint) };
            var content = new StringContent(jsonLog, Encoding.UTF8, "application/json");

            var result = client.PostAsync(_logSettings.LogRoute, content).Result;
        }
    }
}
