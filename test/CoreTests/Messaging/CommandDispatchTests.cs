using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Test.Container;
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
        [Fact(DisplayName = "Can Send Command with Async Handler returning Result")]
        public void CanSendCommand_WithAsyncHandler_ReturningResult()
        {
            MockCommandResult cmdResult = null;

            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHostCommandConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())

                .Act.OnContainer(async c => {
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockCommand();

                    cmdResult = await domainEventSrv.SendAsync(evt);
                })
                .Result.Assert
                    .Container(c =>
                    {
                        cmdResult.Should().NotBeNull();
                        cmdResult.Value.Should().Be("MOCK_VALUE");

                        var consumer = c.Services.Resolve<MockCommandConsumer>();
                        consumer.ExecutedHandlers.Should().HaveCount(1);
                        consumer.ExecutedHandlers.Should().Contain("OnCommand");
                    });
            });               
        }

        /// <summary>
        /// A command domain event can have only one event consumer handler.
        /// If there are more than one event handler, an exception is raised.
        /// </summary>
        [Fact(DisplayName = nameof(CommandMessagesCanOnly_HaveOneEventHandler))]
        public void CommandMessagesCanOnly_HaveOneEventHandler()
        {
             ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHostCommandConsumer();
                        r.AddMultipleConsumers();
                    })
                    .Container(c => c.UsingDefaultServices())

                .Act.OnContainer(async c => {
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockCommand();
                    await domainEventSrv.SendAsync(evt);
                })
                .Result.Assert
                    .Exception<PublisherException>(ex =>
                    {
                        ex.Message.Should().Contain(
                            "Exception publishing message.  See log for details.");
                    });
            });                
        }

        [Fact(DisplayName = nameof(CommandResult_NotRequired))]
        public void CommandResult_NotRequired()
        {
        
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHostCommandConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())

                .Act.OnContainer(c => {
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockCommandNoResult();

                    domainEventSrv.SendAsync(evt);
                })
                .Assert
                    .Container(c =>
                    {
                        var consumer = c.Services.Resolve<MockCommandConsumer>();
                        consumer.ExecutedHandlers.Should().HaveCount(1);
                        consumer.ExecutedHandlers.Should().Contain("OnCommandNoResult");
                    });
            });           
        }
    }
}
