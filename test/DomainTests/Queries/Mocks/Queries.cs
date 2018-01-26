using NetFusion.Messaging.Types;
using System.Collections.Generic;

namespace DomainTests.Queries.Mocks
{
    public class TestQuery : Query<TestQueryResult>
    {
        public List<string> TestLog = new List<string>();
    }

    public class TestQueryResult
    {

    }

    public class TestQueryNoConsumer : Query<TestQueryResult>
    {
    }
}
