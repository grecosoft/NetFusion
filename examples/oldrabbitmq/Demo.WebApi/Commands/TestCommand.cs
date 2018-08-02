using NetFusion.Messaging.Types;

namespace Demo.WebApi.Commands
{
    public class TestCommand : Command
    {
        public TestCommand()
        {
            this.SetRouteKey("GeneratePdf");
        }
    }
}