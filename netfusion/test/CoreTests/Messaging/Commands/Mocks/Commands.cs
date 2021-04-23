using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.Commands.Mocks
{
    public class MockCommand : Command<MockCommandResult>
    {
    }

    public class MockCommandNoResult : Command
    {

    }

    public class MockCommandResult
    {
        public string Value { get; set; }
    }
}