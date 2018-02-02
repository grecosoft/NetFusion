using System.Collections;

namespace NetFusion.Web.Mvc.Composite.Models
{
    /// <summary>
    /// Model representing a plugin module log.  Since the log for a
    /// given module is specific to that module, the log is returned
    /// as a simple dictionary of key/values pairs to the client.
    /// </summary>
    public class PluginModule
    {
        public string Name { get; set; }
        public IDictionary Log { get; set; }
    }
}
