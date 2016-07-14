using NetFusion.Logging.Configs;
using RestSharp;
using System.Collections.Generic;

namespace NetFusion.Logging.Core
{
    public class CompositeLogger : ICompositeLogger
    {
        private readonly CompositeLogSettings _logSettings;

        public CompositeLogger(CompositeLogSettings logSettings)
        {
            _logSettings = logSettings;
        }

        public void Log(IDictionary<string, object> log)
        {
            if (!_logSettings.SendLog) return;

            var client = new RestClient(_logSettings.Endpoint);
            var request = new RestRequest(_logSettings.LogRoute, Method.POST);

            request.AddJsonBody(log);
            client.Execute(request);
        }
    }
}
