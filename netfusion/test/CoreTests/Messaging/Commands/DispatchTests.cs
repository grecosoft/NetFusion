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
        /// Dispatched command can be handled by asynchronous consumer method returning a result.
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
                        var cmd = new MockCommand();
                        
                        cmdResult = await s.GetRequiredService<IMessagingService>()
                            .SendAsync(cmd);
                    });

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    cmdResult.Should().NotBeNull();
                    cmdResult.Value.Should().Be("MOCK_ASYNC_VALUE");
                    
                    log.Entries.Should().HaveCount(1);
                    log.Entries.Should().Contain("Async-Command-Handler");
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
                        var cmd = new MockCommand();
                        
                        cmdResult = await s.GetRequiredService<IMessagingService>()
                            .SendAsync(cmd);
                    });

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    cmdResult.Should().NotBeNull();
                    cmdResult.Value.Should().Be("MOCK_SYNC_VALUE");

                    log.Entries.Should().HaveCount(1);
                    log.Entries.Should().Contain("Sync-Command-Handler");
                });
            });
        }

        /// <summary>
        /// Commands can be handled by a consumer not requiring a response.
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
                        var cmd = new MockCommandNoResult();
                        
                        await s.GetRequiredService<IMessagingService>()
                            .SendAsync(cmd);
                    });

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    log.Entries.Should().HaveCount(1);
                    log.Entries.Should().Contain("Async-Command-Handler-No-Result");
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
                        var cmd = new MockCommand();
                        
                        return s.GetRequiredService<IMessagingService>()
                            .SendAsync(cmd);
                    });

                testResult.Assert.Exception((PublisherException ex) =>
                {
                    Assert.Contains("Exception when invoking message publishers", ex.Message);
                });
            });
        }

        [Fact]
        public void CommandMessages_MustHave_CommandHandler()
        {
            
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
