using Microsoft.Extensions.Logging;

namespace NetFusion.Testing.Logging
{
    /// <summary>
    /// Logger factory that creates an instance of the TestLogger.
    /// </summary>
    public class TestLoggerFactory : ILoggerFactory
    {
        public TestLogger Logger { get; } = new TestLogger();

        public ILogger CreateLogger(string categoryName)
        {
            return Logger;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public void Dispose()
        {
            
        }
    }
}
