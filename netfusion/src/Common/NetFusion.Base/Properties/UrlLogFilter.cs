using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NetFusion.Base.Properties
{
    /// <summary>
    /// Class containing properties for and Url to be excluded from logs.  This can be 
    /// used to suppress frequently received calls for such as health and readiness checks. 
    /// </summary>
    public class UrlLogFilter
    {
        public static readonly string Key = typeof(UrlLogFilter).FullName;
        
        /// <summary>
        /// The URL to exclude.
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// The result status that should be excluded.
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }
    }

    /// <summary>
    ///  Extensions for managing list of URLs to be excluded from logs.
    /// </summary>
    public static class LogFilterExtensions
    {
        /// <summary>
        /// Adds filter to dictionary containing a list of URLs / Statuses that
        /// should be excluded from logging.
        /// </summary>
        /// <param name="values">Properties dictionary containing list of associated key/value pairs.</param>
        /// <param name="url">The URL request path to the excluded.</param>
        /// <param name="statusCode">The specific response status code that should be excluded.</param>
        public static void AddLogUrlFilter(this IDictionary<string, object> values,
            string url, HttpStatusCode? statusCode = null)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Url not specified.", nameof(url));
            
            values.TryAdd(UrlLogFilter.Key, new List<UrlLogFilter>());

            if (values[UrlLogFilter.Key] is not List<UrlLogFilter> urlLogFilters) return;
            
            var urlFilter = urlLogFilters.FirstOrDefault(f => f.Url == url) ?? new UrlLogFilter();
            urlFilter.Url = url;
            urlFilter.StatusCode = statusCode;
            
            urlLogFilters.Add(urlFilter);
        }

        /// <summary>
        /// Determines if a given url with an optional status code is registered for
        /// exclusion from logs.
        /// </summary>
        /// <param name="values">Properties dictionary containing list of associated key/value pairs.</param>
        /// <param name="url">The URL request path to check.</param>
        /// <param name="statusCode">The optional status code to check.</param>
        /// <returns></returns>
        public static bool IsLogUrlFilter(this IDictionary<string, object> values,
            string url, HttpStatusCode? statusCode = null)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Url not specified.", nameof(url));


            return values[UrlLogFilter.Key] is List<UrlLogFilter> urlLogFilters 
                   && urlLogFilters.Any(f => f.Url == url && f.StatusCode == statusCode);
        }
    }
}