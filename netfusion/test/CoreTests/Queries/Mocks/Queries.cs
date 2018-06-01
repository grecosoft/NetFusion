using NetFusion.Messaging.Types;
using System.Collections.Generic;

namespace CoreTests.Queries.Mocks
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
