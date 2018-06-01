using System;
using System.Collections;

namespace NetFusion.Web.Mvc.Composite.Models
{
    /// <summary>
    /// Model containing summary information for a given plugin.
    /// </summary>
    public class PluginSummary 
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Assembly { get; private set; }

        public static PluginSummary FromLog(IDictionary logEntry)
        {
            if (logEntry == null)
            {
                throw new ArgumentNullException(nameof(logEntry));
            }

            return new PluginSummary
            {
                Id = logEntry["Plugin:Id"] as string,
                Name = logEntry["Plugin:Name"] as string,
                Assembly = logEntry["Plugin:Assembly"] as string
            };
        }
    }
}