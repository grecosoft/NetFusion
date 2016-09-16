using NetFusion.Common;
using Serilog;

namespace NetFusion.Logging.Serilog
{
    public static class SerilogExtensions
    {
        /// <summary>
        /// Extension method that creates a context for which the 
        /// PluginEnricher will use to enrich the log with the
        /// corresponding plug-in properties.
        /// </summary>
        /// <typeparam name="T">The context type used to determine associated plugin.</typeparam>
        /// <param name="logger">The logger being extended.</param>
        /// <returns></returns>
        public static ILogger ForPluginContext<T>(this ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            return logger.ForContext(
                SerilogManifest.ContextPropName, 
                typeof(T).AssemblyQualifiedName);
        }
    }
}
