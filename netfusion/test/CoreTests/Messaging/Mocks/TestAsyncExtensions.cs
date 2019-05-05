using NetFusion.Test.Plugins;
using System.Threading.Tasks;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Plugin;

namespace CoreTests.Messaging.Mocks
{
    public static class TestAsyncExtensions
    {
        public static CompositeContainer WithHostAsyncConsumer(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType<MockAsyncMessageConsumer>();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();

            return container;
        }
    }

    //-------------------------- MOCKED TYPED --------------------------------------
    public class MockAsyncMessageConsumer : MockConsumer,
            IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEvent1Async(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent1Async));
            return Task.Run(() =>
            {

            });
        }

        [InProcessHandler]
        public Task OnEvent2Async(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent2Async));
            return Task.Run(() =>
            {

            });
        }

        [InProcessHandler]
        public void OnEvent3(MockDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEvent3));
        }
    }

}
