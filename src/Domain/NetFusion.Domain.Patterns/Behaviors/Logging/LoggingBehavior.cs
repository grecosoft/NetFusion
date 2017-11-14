using Microsoft.Extensions.Logging;
using NetFusion.Domain.Entities.Core;

namespace NetFusion.Domain.Patterns.Behaviors.Logging
{
    public class LoggingBehavior : ILoggingBehavior
    {
        private readonly IBehaviorDelegator _entity;
        
        // Collaborations:
        public ILoggerFactory LoggerFactory { get; set; }

        public ILogger Logger { get; }

        public LoggingBehavior(IBehaviorDelegator entity)
        {
            _entity = entity;

            Logger = LoggerFactory.CreateLogger(_entity.GetType());
        }
    }
}
