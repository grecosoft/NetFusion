using NetFusion.Messaging;

namespace CoreTests.Queries.Mocks
{
    public class TestConsumer : IQueryConsumer
    {
        public TestQueryResult Execute(TestQuery query)
        {
            query.TestLog.Add(nameof(TestConsumer));
            return new TestQueryResult
            {

            };
        }
    }

    public class DuplicateConsumerOne : IQueryConsumer
    {
        public TestQueryResult Execute(TestQuery query)
        {
            return new TestQueryResult
            {

            };
        }
    }

    public class DuplicateConsumerTwo : IQueryConsumer
    {
        public TestQueryResult Execute(TestQuery query)
        {
            return new TestQueryResult
            {

            };
        }
    }
}
