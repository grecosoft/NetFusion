﻿using Microsoft.Extensions.Logging;
using Moq;
using NetFusion.Base.Scripting;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Core;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockMessageBroker : MessageBroker
    {
        public MockMessageBroker(
            ILoggerFactory logger, 
            Mock<IMessageDispatchModule> mockMsgModule) :

            base(logger, mockMsgModule.Object,
                new Mock<IBrokerMetaRepository>().Object,
                new Mock<IEntityScriptingService>().Object)
        {
      
        }
    }
}
