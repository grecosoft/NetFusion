using System.Threading.Tasks;
using CoreTests.Messaging.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types;
using NetFusion.Test.Plugins;
// ReSharper disable All

namespace CoreTests.Messaging.Commands
{
    public static class TestSetupExtensions
    {
        public static CompositeContainer WithHostCommandConsumer(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType<MockCommandConsumer>();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();
            
            return container;
        }

        public static CompositeContainer AddMultipleConsumers(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockInvalidCommandConsumer>();
            
            container.RegisterPlugins(appPlugin);
            container.RegisterPlugin<MessagingPlugin>();
            
            return container;
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
