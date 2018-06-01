using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.Test.Plugins;
using System.Threading.Tasks;

namespace CoreTests.Messaging.Mocks
{
    public static class TestCommandSetupExtensions
    {
        public static TestTypeResolver WithHostCommandConsumer(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType<MockCommand>()
                .AddPluginType<MockCommandConsumer>();

            resolver.AddPlugin<MockCorePlugin>()
                .UseMessagingPlugin();

            return resolver;
        }

        public static TestTypeResolver AddMultipleConsumers(this TestTypeResolver resolver)
        {
            resolver.GetHostPlugin()
               .AddPluginType<MockInvalidCommandConsumer>();

            return resolver;
        }
    }

    //-------------------------- MOCKED TYPED --------------------------------------

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

    public class MockCommandConsumer : MockConsumer, IMessageConsumer
    {
        [InProcessHandler]
        public async Task<MockCommandResult> OnCommand(MockCommand evt)
        {
            AddCalledHandler(nameof(OnCommand));

            var mockResponse = new MockCommandResult();

            await Task.Run(() => {
                mockResponse.Value = "MOCK_VALUE";
            });

            return mockResponse;
        }

        [InProcessHandler]
        public Task OnCommandNoResult(MockCommandNoResult command)
        {
            AddCalledHandler(nameof(OnCommandNoResult));
            return Task.Run(() => { });
        }
    }

    public class MockInvalidCommandConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public void InvalidHandler1(MockCommand evt)
        {

        }
    }
}
