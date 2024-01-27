using NetFusion.Messaging.Types;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

public class TestCommand : Command;

public class TestCommandConsumer
{
    public void OnCommand(TestCommand command)
    {
        
    }
}