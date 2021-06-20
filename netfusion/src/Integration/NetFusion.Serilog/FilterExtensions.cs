using System.Net;
using NetFusion.Base.Properties;
using NetFusion.Bootstrap.Container;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace NetFusion.Serilog
{
    public static class FilterExtensions
    {
        /// <summary>
        /// Extension method used during Serilog configuration to suppress the logging
        /// of any reoccurring calls.
        /// </summary>
        /// <param name="config">Serilog filter configuration.</param>
        /// <returns>Serilog log configuration.</returns>
        public static LoggerConfiguration SuppressReoccurringRequestEvents(this LoggerFilterConfiguration config)
        {
            return config.ByExcluding(e =>
            {
                string requestPath = GetRequestPath(e);
                HttpStatusCode? statusCode = GetStatusCode(e);

                if (requestPath != null && statusCode != null)
                {
                    return CompositeApp.Instance.Properties.IsLogUrlFilter(requestPath, statusCode);
                }

                return false;
            });
        }
        
        // Inspects the Serilog event to read the requested URL:
        private static string GetRequestPath(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue("RequestPath", out var value) 
                && value is ScalarValue sv 
                && sv.Value is string requestPath)
            {
                return requestPath;
            }

            return null;
        }
        
        // Inspects the Serilog event to read the response status code:
        private static HttpStatusCode? GetStatusCode(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue("StatusCode", out var value) 
                && value is ScalarValue sv 
                && sv.Value is int statusCode)
            {
                return (HttpStatusCode)statusCode;
            }

            return null;
        }
    }
}