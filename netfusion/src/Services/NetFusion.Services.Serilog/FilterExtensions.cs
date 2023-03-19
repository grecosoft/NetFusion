using System.Net;
using NetFusion.Common.Base.Properties;
using NetFusion.Core.Bootstrap.Container;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace NetFusion.Services.Serilog;

public static class FilterExtensions
{
    /// <summary>
    /// Extension method used during Serilog configuration to suppress the logging
    /// of any reoccurring calls.  This can be used to filter out reoccurring calls
    /// to health-check URLs.
    /// </summary>
    /// <param name="config">Serilog filter configuration.</param>
    /// <returns>Serilog log configuration.</returns>
    public static LoggerConfiguration SuppressReoccurringRequestEvents(this LoggerFilterConfiguration config)
    {
        return config.ByExcluding(e =>
        {
            string? requestPath = GetRequestPath(e);
            HttpStatusCode? statusCode = GetStatusCode(e);

            if (requestPath != null && statusCode != null)
            {
                return CompositeApp.Instance.Properties.IsLogUrlFilter(requestPath, statusCode);
            }

            return false;
        });
    }
        
    // Inspects the Serilog event to read the requested URL:
    private static string? GetRequestPath(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("RequestPath", out var value) 
            && value is ScalarValue { Value: string requestPath })
        {
            return requestPath;
        }

        return null;
    }
        
    // Inspects the Serilog event to read the response status code:
    private static HttpStatusCode? GetStatusCode(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("StatusCode", out var value) 
            && value is ScalarValue { Value: int statusCode })
        {
            return (HttpStatusCode)statusCode;
        }

        return null;
    }
}