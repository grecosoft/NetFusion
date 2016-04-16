using System.Collections.Generic;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public abstract class MockConsumer
    {
        private readonly IList<string> _executedHandlers = new List<string>();

        public IEnumerable<string> ExecutedHandlers
        {
            get { return _executedHandlers; }
        }

        protected void AddCalledHandler(string handlerName)
        {
            _executedHandlers.Add(handlerName);
        }
    }
}
