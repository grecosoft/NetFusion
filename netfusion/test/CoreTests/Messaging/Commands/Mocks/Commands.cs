using System.Collections.Generic;
using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.Commands.Mocks
{
    public class MockCommand : Command<MockCommandResult>
    {
        public List<string> ThrowInHandlers { get; } = new();
    }

    public class MockCommandNoResult : Command
    {

    }

    public class MockCommandNoHandler : Command
    {
        
    }

    public class MockCommandResult
    {
        public string Value { get; set; }
    }
}