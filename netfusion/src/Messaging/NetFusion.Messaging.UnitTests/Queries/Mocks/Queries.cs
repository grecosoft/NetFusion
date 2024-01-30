using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.UnitTests.Queries.Mocks;

public class MockQuery : Query<MockQueryResult>
{
    public List<string> QueryAsserts { get; } = [];
    public List<string> ThrowInHandlers { get; } = [];
}

public class MockQueryResult;