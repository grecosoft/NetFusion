using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.UnitTests.DomainEvents;
using NetFusion.Messaging.UnitTests.DomainEvents.Mocks;
using NetFusion.Messaging.UnitTests.Messaging;

// ReSharper disable All

namespace CoreTests.Messaging.DomainEvents;

/// <summary>
/// Tests validating exceptions, thrown from domain-event consumers, are correctly recorded.  Each invoked
/// message handler, resulting in an exception, is captured as a MessageDispatchException.  The consumer,
/// publishing the messaged, will receive PublisherException containing a collection of child PublisherException
/// instances for each IMessagePublisher resulting in an exception.  Each child Publisher exception in turn will
/// have the a list of associated MessageDispatchException instances.
///
/// The root PublisherException contains a Details property containing key/value pairs of information recorded by
/// the contained exceptions.  The Details property contains additional information for each recorded exception.
/// The NetFusion.Serilog plugin writes the Details property as a structured error message.
/// </summary>
public class DispatchExceptionTests
{
    /// <summary>
    /// Validates that an exception thrown by a synchronous domain-event handler is correctly captured. 
    /// </summary>
    [Fact]
    public Task ExceptionsCaptured_ForSync_Consumer()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var domainEvt = new MockDomainEvent();
                
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithSyncDomainEventHandler())
                .State(() =>
                {
                    domainEvt.ThrowInHandlers.Add(nameof(MockSyncDomainEventConsumerOne));
                })
                .Act.RecordException().OnServicesAsync(async s =>
                { 
                    await s.GetRequiredService<IMessagingService>()
                        .PublishAsync(domainEvt);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                Assert.NotNull(ex.InnerException);
                AssertCapturedException(ex.InnerException, typeof(MockSyncDomainEventConsumerOne));
            });
        });
    }
        
    /// <summary>
    /// Validates that an exception thrown by a asynchronous domain-event handler is correctly captured. 
    /// </summary>
    [Fact]
    public Task ExceptionsCaptured_ForAsync_Consumer()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var domainEvt = new MockDomainEvent();
                
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithAsyncDomainEventHandler())
                .State(() =>
                {
                    domainEvt.ThrowInHandlers.Add(nameof(MockAsyncDomainEventConsumerOne));
                })
                .Act.RecordException().OnServicesAsync(async s =>
                {
                    await s.GetRequiredService<IMessagingService>()
                        .PublishAsync(domainEvt);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                Assert.NotNull(ex.InnerException);
                AssertCapturedException(ex.InnerException, typeof(MockAsyncDomainEventConsumerOne));
            });
        });
    }
        
    /// <summary>
    /// Validates that exceptions, thrown by a multiple synchronous and asynchronous domain-event handlers,
    /// are correctly captured. 
    /// </summary>
    [Fact]
    public Task ExceptionsCapture_ForMultipleAsyncAndSync_Consumers()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var domainEvt = new MockDomainEvent();
                
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithMultipleDomainEventHandlers())
                .State(() =>
                {
                    domainEvt.ThrowInHandlers.Add(nameof(MockSyncDomainEventConsumerOne));
                    domainEvt.ThrowInHandlers.Add(nameof(MockAsyncDomainEventConsumerOne));
                })
                .Act.RecordException().OnServicesAsync(async s =>
                {
                    await s.GetRequiredService<IMessagingService>()
                        .PublishAsync(domainEvt);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                var parentDispatchEx = ex.InnerException as MessageDispatchException;
                parentDispatchEx.Should().NotBeNull();
                parentDispatchEx!.ChildExceptions.Should().HaveCount(2);
                    
                var syncDispatchEx =  parentDispatchEx.ChildExceptions.ElementAt(0) as MessageDispatchException;
                syncDispatchEx.Should().NotBeNull();
                    
                var asyncDispatchEx =  parentDispatchEx.ChildExceptions.ElementAt(1) as MessageDispatchException;
                asyncDispatchEx.Should().NotBeNull();
                    
                AssertCapturedException(syncDispatchEx!, typeof(MockSyncDomainEventConsumerOne));
                AssertCapturedException(asyncDispatchEx!, typeof(MockAsyncDomainEventConsumerOne));
            });
        });
    }
        
    /// <summary>
    /// Tests the scenario where a parent message handler dispatches a child message resulting in an exception.
    /// In this case, the ChildExceptions and Details are captured in the order the messages where published.
    /// </summary>
    [Fact]
    public Task ExceptionsCaptured_ForChildPublished_Message()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithChildMessageHandlerException())
                .Act.RecordException().OnServicesAsync(async s =>
                {
                    var domainEvt = new MockDomainEvent();
                        
                    await s.GetRequiredService<IMessagingService>()
                        .PublishAsync(domainEvt);
                });

            testResult.Assert.Exception<PublisherException>(ex =>
            {
                ex.InnerException.Should().BeOfType<MessageDispatchException>();
                ex.InnerException?.InnerException.Should().BeOfType<PublisherException>();
                ex.InnerException?.InnerException?.InnerException.Should().BeOfType<MessageDispatchException>();
                ex.InnerException?.InnerException?.InnerException?.InnerException.Should().BeOfType<InvalidOperationException>();
            });
        });
    }
        
    private static void AssertCapturedException(Exception ex, Type expectedConsumerType)
    {
        ex.Should().NotBeNull();

        var dispatchEx = ex as MessageDispatchException;
        dispatchEx.Should().NotBeNull();
            
        var sourceEx = dispatchEx?.InnerException as InvalidOperationException;
        sourceEx.Should().NotBeNull();
        sourceEx!.Message.Should().Be($"{expectedConsumerType.Name}_Exception");
    }
}