using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging.Exceptions;
using NetFusion.Test.Container;
using System.Threading.Tasks;
using NetFusion.Messaging;
using Xunit;

namespace CoreTests.Messaging
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
        public Task CanSendCommand_WithAsyncHandler_ReturningResult()
        {
            MockCommandResult cmdResult = null;

            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHostCommandConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetService<IMessagingService>();
                        var cmd = new MockCommand();

                        cmdResult = await messagingSrv.SendAsync(cmd);
                    });

                testResult.Assert.Services(s =>
                {
                    cmdResult.Should().NotBeNull();
                    cmdResult.Value.Should().Be("MOCK_VALUE");

                    var consumer = s.GetRequiredService<MockCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommand");
                });
            });
        }

        [Fact(DisplayName = nameof(CommandResult_NotRequired))]
        public Task CommandResult_NotRequired()
        {          
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHostCommandConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetService<IMessagingService>();
                        var cmd = new MockCommandNoResult();

                        await messagingSrv.SendAsync(cmd);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommandNoResult");
                });
            });
        }

        [Fact(DisplayName = nameof(CommandMessagesCanOnly_HaveOneEventHandler))]
        public Task CommandMessagesCanOnly_HaveOneEventHandler()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHostCommandConsumer().AddMultipleConsumers())
                    .Act.RecordException().OnServicesAsync(s =>
                    {
                        var messagingSrv = s.GetService<IMessagingService>();
                        var evt = new MockCommand();
                        return messagingSrv.SendAsync(evt);
                    });

                testResult.Assert.Exception((PublisherException ex) =>
                {
                    Assert.Contains("Exception when invoking message publishers", ex.Message);
                });
            });
        }
    }
}
