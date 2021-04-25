using System.Threading.Tasks;
using CoreTests.Messaging.Commands.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging.Commands
{
    /// <summary>
    /// Unit tests for command message types.  A command can have one and only
    /// one associated message handler and can optionally return a result.
    /// </summary>
    public class DispatchTests
    {
        /// <summary>
        /// Dispatched command can be handled by asynchronous consumer method
        /// returning a result.
        /// </summary>
        [Fact]
        public Task CanSendCommand_WithAsyncHandler_ReturningResult()
        {
            MockCommandResult cmdResult = null;

            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithAsyncCommandConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var cmd = new MockCommand();

                        cmdResult = await messagingSrv.SendAsync(cmd);
                    });

                testResult.Assert.Services(s =>
                {
                    cmdResult.Should().NotBeNull();
                    cmdResult.Value.Should().Be("MOCK_VALUE");

                    var consumer = s.GetRequiredService<MockAsyncCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommand");
                });
            });
        }

        /// <summary>
        /// When sending commands, the handler method can be either synchronous or asynchronous.
        /// The message dispatcher normalizes command handler calls to be asynchronous from the
        /// perspective of the calling code.  This allows the implementation of the command
        /// handler to change without having to refactor any of the calling code.
        /// </summary>
        [Fact]
        public Task CanSendCommand_WithSyncHandler_ReturningResult()
        {
            MockCommandResult cmdResult = null;

            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithSyncCommandConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var cmd = new MockCommand();

                        cmdResult = await messagingSrv.SendAsync(cmd);
                    });

                testResult.Assert.Services(s =>
                {
                    cmdResult.Should().NotBeNull();
                    cmdResult.Value.Should().Be("MOCK_SYNC_VALUE");

                    var consumer = s.GetRequiredService<MockSyncCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommand");
                });
            });
        }

        /// <summary>
        /// Commands can be sent for which the handler does not return a response.
        /// </summary>
        [Fact]
        public Task CommandResult_NotRequired()
        {          
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithAsyncCommandConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var cmd = new MockCommandNoResult();

                        await messagingSrv.SendAsync(cmd);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockAsyncCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommandNoResult");
                });
            });
        }

        /// <summary>
        /// Unlike domain-event messages, commands can have one and only one handler.
        /// </summary>
        [Fact]
        public Task CommandMessagesCanOnly_HaveOneEventHandler()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithMultipleConsumers())
                    .Act.RecordException().OnServicesAsync(s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var evt = new MockCommand();
                        return messagingSrv.SendAsync(evt);
                    });

                testResult.Assert.Exception((PublisherException ex) =>
                {
                    Assert.Contains("Exception when invoking message publishers", ex.Message);
                });
            });
        }

        [Fact]
        public void ExceptionsCaptured_ForAsync_Consumer()
        {
            
        }
        
        [Fact]
        public void ExceptionsCaptured_ForSync_Consumer()
        {
            
        }
    }
}
