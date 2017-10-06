using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Threading.Tasks;
using Xunit;

namespace BootstrapTests.Messaging
{
    /// <summary>
    /// Unit tests for command message types.  A command can only have one associated message
    /// handler and can optionally return a result.
    /// </summary>
    public class CommandDispatchTests
    {
        /// <summary>
        /// Command domain events can have a return result. 
        /// </summary>
        [Fact(DisplayName = nameof(CommandWithResult_FromAsyncHandler))]
        public Task CommandWithResult_FromAsyncHandler()
        {
            MockCommandResult result = null;

            return CommandConsumer.Test(
                async c => {

                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockCommand();
                    var futureResult = domainEventSrv.PublishAsync(evt);

                    result = await futureResult;
                }, 
                (IAppContainer c) => {
                    result.Should().NotBeNull();
                    result.Value.Should().Be("MOCK_VALUE");

                    var consumer = c.Services.Resolve<MockCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommand");
                });
        }

        [Fact(DisplayName = nameof(CommandResult_NotRequired))]
        public Task CommandResult_NotRequired()
        {
            return CommandConsumer.Test(
                async c =>
                {
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockCommandNoResult();
                    await domainEventSrv.PublishAsync(evt);
                },
                (IAppContainer c) =>
                {
                    var consumer = c.Services.Resolve<MockCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommandNoResult");
                });
        }

        /// <summary>
        /// A command domain event can have only one event consumer handler.
        /// If there are more than one event handler, an exception is raised.
        /// </summary>
        [Fact(DisplayName = nameof(CommandMessagesCanOnly_HaveOneEventHandler))]
        public Task CommandMessagesCanOnly_HaveOneEventHandler()
        {
            return InvalidCommandConsumer.Test(
                async c => {
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockInvalidCommand();
                    await domainEventSrv.PublishAsync(evt);
                }, 
                (c, e) => {
                    e.Should().NotBeNull();
                    e.Should().BeOfType<PublisherException>();
                });
        }


        //--------------------------------TEST SPECIFIC SETUP------------------------------------------//

        public class MockCommand : Command<MockCommandResult>
        {
        }

        public class MockCommandNoResult : Command
        {

        }

        public class MockInvalidCommand : Command<MockCommandResult>
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
            public void InvalidHandler1(MockInvalidCommand evt)
            {

            }

            [InProcessHandler]
            public void InvalidHandler2(MockInvalidCommand evt)
            {

            }
        }

        public static ContainerTest CommandConsumer => ContainerSetup
            .Arrange((TestTypeResolver config) =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockCommand>()
                    .AddPluginType<MockCommandConsumer>();

                config.AddPlugin<MockCorePlugin>()
                    .UseMessagingPlugin();
            }, c =>
            {
                c.WithConfig<AutofacRegistrationConfig>(regConfig =>
                {
                    regConfig.Build = builder =>
                    {
                        builder.RegisterType<NullEntityScriptingService>()
                            .As<IEntityScriptingService>()
                            .SingleInstance();
                    };
                });
            });


        public static ContainerTest InvalidCommandConsumer => ContainerSetup
            .Arrange((TestTypeResolver config) =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockInvalidCommand>()
                    .AddPluginType<MockInvalidCommandConsumer>();

                config.AddPlugin<MockCorePlugin>()
                    .UseMessagingPlugin();
            }, c =>
            {
                c.WithConfig<AutofacRegistrationConfig>(regConfig =>
                {
                    regConfig.Build = builder =>
                    {
                        builder.RegisterType<NullEntityScriptingService>()
                            .As<IEntityScriptingService>()
                            .SingleInstance();
                    };
                });
            });
    }
}
