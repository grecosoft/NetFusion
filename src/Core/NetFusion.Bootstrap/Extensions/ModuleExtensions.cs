using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Extensions
{
    public static class ModuleExtensions
    {
        /// <summary>
        /// Returns modules that are not marked as being executed.  This is for
        /// use during development to disable a given plug-in module that is 
        /// currently being developed and not complete.
        /// </summary>
        /// <param name="plugin">The source plug in.</param>
        /// <returns>List of included modules.</returns>
        public static IEnumerable<IPluginModule> IncludedModules(this Plugin plugin)
        {
            Check.NotNull(plugin, nameof(plugin));

            return plugin.PluginModules.Where(m => !m.IsExcluded);
        }
    }
}
