using System.Collections.Generic;
using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.Queries.Mocks
{
    public class MockQuery : Query<MockQueryResult>
    {
        public List<string> QueryAsserts { get; } = new();
        public List<string> ThrowInHandlers { get; } = new();
    }

    public class MockQueryResult
    {

    }
}
