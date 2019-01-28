﻿using NetFusion.Messaging;

namespace CoreTests.Queries.Mocks
{
    public class TestConsumer : IQueryConsumer
    {
        [InProcessHandler]
        public TestQueryResult Execute(TestQuery query)
        {
            query.TestLog.Add(nameof(TestConsumer));
            return new TestQueryResult();
        }
    }

    public class DuplicateConsumerOne : IQueryConsumer
    {
        [InProcessHandler]
        public TestQueryResult Execute(TestQuery query)
        {
            return new TestQueryResult();
        }
    }

    public class DuplicateConsumerTwo : IQueryConsumer
    {
        [InProcessHandler]
        public TestQueryResult Execute(TestQuery query)
        {
            return new TestQueryResult();
        }
    }
}
