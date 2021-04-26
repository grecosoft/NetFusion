using System;
using System.Threading.Tasks;
using CoreTests.Messaging.Commands.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Test.Container;
using Xunit;
// ReSharper disable All

namespace CoreTests.Messaging.Commands
{
    public class DispatchExceptionTests
    {
        [Fact]
        public Task CommandMessages_MustHave_CommandHandler()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithMultipleConsumers())
                    .Act.RecordException().OnServicesAsync(s =>
                    {
                        var cmd = new MockCommandNoHandler();
                        
                        return s.GetRequiredService<IMessagingService>()
                            .SendAsync(cmd);
                    });

                testResult.Assert.Exception((PublisherException ex) =>
                {
                    ex.InnerException?.Message.Should().Contain("must have one and only one consumer");
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
                    ex.InnerException?.Message.Should().Contain("must have one and only one consumer");
                });
            });
        }

        /// <summary>
        /// Validates that an exception thrown by a asynchronous command handler is correctly captured. 
        /// </summary>
        [Fact]
        public Task ExceptionsCaptured_ForAsync_Consumer()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var command = new MockCommand();
                
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithAsyncCommandConsumer())
                    .State(() =>
                    {
                        command.ThrowInHandlers.Add(nameof(MockAsyncCommandConsumer));
                    })
                    .Act.RecordException().OnServicesAsync(async s =>
                    { 
                        await s.GetRequiredService<IMessagingService>()
                            .SendAsync(command);
                    });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    AssertCapturedException(ex.InnerException, typeof(MockAsyncCommandConsumer));
                });
            });
        }
        
        /// <summary>
        /// Validates that an exception thrown by a synchronous command handler is correctly captured. 
        /// </summary>
        [Fact]
        public Task ExceptionsCaptured_ForSync_Consumer()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var command = new MockCommand();
                
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithSyncCommandConsumer())
                    .State(() =>
                    {
                        command.ThrowInHandlers.Add(nameof(MockSyncCommandConsumer));
                    })
                    .Act.RecordException().OnServicesAsync(async s =>
                    { 
                        await s.GetRequiredService<IMessagingService>()
                            .SendAsync(command);
                    });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    AssertCapturedException(ex.InnerException, typeof(MockSyncCommandConsumer));
                });
            });
        }
        
        private static void AssertCapturedException(Exception ex, Type expectedConsumerType)
        {
            ex.Should().NotBeNull();

            var dispatchEx = ex as MessageDispatchException;
            dispatchEx.Should().NotBeNull();
            
            var sourceEx = dispatchEx.InnerException as InvalidOperationException;
            sourceEx.Should().NotBeNull();
            sourceEx.Message.Should().Be($"{expectedConsumerType.Name}_Exception");
        }
    }
}