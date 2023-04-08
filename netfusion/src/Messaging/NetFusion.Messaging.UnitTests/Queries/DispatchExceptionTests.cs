using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.UnitTests;
using NetFusion.Messaging.UnitTests.Queries;
using NetFusion.Messaging.UnitTests.Queries.Mocks;

// ReSharper disable All

namespace NetFusion.Messaging.Tests.Queries;

public class DispatchExceptionTests
{
    /// <summary>
    /// Validates that an exception thrown by a asynchronous command handler is correctly captured. 
    /// </summary>
    [Fact]
    public Task ExceptionsCapture_ForDispatched_AsyncConsumer()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var query = new MockQuery();
                
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithAsyncQueryConsumer())
                .State(() =>
                {
                    query.ThrowInHandlers.Add(nameof(MockAsyncQueryConsumer));
                })
                .Act.RecordException().OnServicesAsync(async s =>
                { 
                    await s.GetRequiredService<IMessagingService>()
                        .DispatchAsync(query);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                ex.InnerException.Should().BeOfType<MessageDispatchException>();
                ex.InnerException.InnerException.Should().BeOfType<InvalidOperationException>();
                ex.InnerException.InnerException.Message.Should().Be($"{nameof(MockAsyncQueryConsumer)}_Exception");
            });
        });
    }

    /// <summary>
    /// Validates that an exception thrown by a synchronous command handler is correctly captured. 
    /// </summary>
    [Fact]
    public Task ExceptionsCapture_ForDispatched_SyncConsumer()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var query = new MockQuery();
                
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithSyncQueryConsumer())
                .State(() =>
                {
                    query.ThrowInHandlers.Add(nameof(MockSyncQueryConsumer));
                })
                .Act.RecordException().OnServicesAsync(async s =>
                { 
                    await s.GetRequiredService<IMessagingService>()
                        .DispatchAsync(query);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                ex.InnerException.Should().BeOfType<MessageDispatchException>();
                ex.InnerException.InnerException.Should().BeOfType<InvalidOperationException>();
                ex.InnerException.InnerException.Message.Should().Be($"{nameof(MockSyncQueryConsumer)}_Exception");
            });
        });
    }
}