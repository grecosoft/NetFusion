using NetFusion.Common;
using Serilog;

namespace NetFusion.Logging.Serilog
{
    public static class SerilogExtensions
    {
        public static ILogger ForContextType<T>(this ILogger logger)
        {
            Check.NotNull(logger, nameof(logger));

            return logger.ForContext(
                SerilogManifest.ContextPropName, 
                typeof(T).AssemblyQualifiedName);
        }
    }
}
