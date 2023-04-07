using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.UnitTests.Commands.Mocks;

namespace NetFusion.Messaging.UnitTests.Commands;

public class DispatchExceptionTests
{
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
            
        var sourceEx = dispatchEx!.InnerException as InvalidOperationException;
        sourceEx.Should().NotBeNull();
        sourceEx!.Message.Should().Be($"{expectedConsumerType.Name}_Exception");
    }
}