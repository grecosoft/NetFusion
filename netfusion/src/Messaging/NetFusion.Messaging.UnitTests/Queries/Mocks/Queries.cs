using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.UnitTests.Queries.Mocks;

public class MockQuery : Query<MockQueryResult>
{
    public List<string> QueryAsserts { get; } = new();
    public List<string> ThrowInHandlers { get; } = new();
}

public class MockQueryResult
{

}