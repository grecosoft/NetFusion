using System.Collections.Generic;
using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.Queries.Mocks
{
    public class TestQuery : Query<TestQueryResult>
    {
        public readonly List<string> TestLog = new List<string>();
    }

    public class TestQueryResult
    {

    }

    public class TestQueryNoConsumer : Query<TestQueryResult>
    {
    }
}
