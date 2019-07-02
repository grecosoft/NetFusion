using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NetFusion.Redis.Internal
{
    /// <summary>
    /// Implementation that writes Redis connection logs to
    /// the configured logger.
    /// </summary>
    public class RedisLogTextWriter : TextWriter
    {
        private readonly ILogger _logger;
        public override Encoding Encoding { get; } = Encoding.UTF8;

        public RedisLogTextWriter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override void Write(string value)
        {
            if (! string.IsNullOrWhiteSpace(value))
            {
                _logger.LogTrace(RedisLogEvents.ConnectionEvent, value);
            }
        }
    }
}