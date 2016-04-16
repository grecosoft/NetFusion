using NetFusion.Messaging;
using System.Threading.Tasks;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockCommandConsumer : MockConsumer,
        IMessageConsumer
    {
        public async Task OnCommand(MockCommand evt)
        {
            AddCalledHandler(nameof(OnCommand));

            await Task.Run(()=> {

                evt.Result = new MockCommandResult();

            });
        }

        public void InvalidHandler1(MockCommand evt)
        {

        }

        public void InvalidHandler2(MockCommand evt)
        {

        }
    }
}
