using NetFusion.Messaging;
using NetFusion.Test.Plugins;
using System.Threading.Tasks;

namespace CoreTests.Messaging.Mocks
{
    public static class TestAsyncExtensions
    {
        public static TestTypeResolver WithHostAsyncConsumer(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType<MockDomainEvent>()
                .AddPluginType<MockAsyncMessageConsumer>();

            resolver.AddPlugin<MockCorePlugin>()
                .UseMessagingPlugin();

            return resolver;
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
