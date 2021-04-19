using System.Threading.Tasks;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging
{
    public class ExceptionDispatchTests
    {
        /// <summary>
        /// Tests that a message handler handler resulting in an exception
        /// is correctly captured.
        /// </summary>
        [Fact]
        public Task ExceptionsCapture_ForPublished_Message()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddHost().WithMessageHandlerException())
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var domainEvt = new MockDomainEvent();

                        await messagingSrv.PublishAsync(domainEvt);
                    });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    // Assert that the inner exception is a dispatch exception:
                    ex.InnerException?.InnerException.Should().NotBeNull();
                    // ex.InnerException?.InnerException.Should().BeOfType<MessageDispatchException>();
                });
            });
        }

        /// <summary>
        /// Tests that a message with multiple handlers resulting in an exception
        /// is correctly captured.
        /// </summary>
        [Fact]
        public void ExceptionsCapture_ForMultiple_MessageHandlers()
        {
            
        }
        
        /// <summary>
        /// Tests the scenario where a parent message handler dispatches a child
        /// message resulting in an exception.
        /// </summary>
        [Fact]
        public Task ExceptionsCaptured_ForChildPublished_Message()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddHost().WithChildMessageHandlerException())
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var domainEvt = new MockDomainEvent();

                        await messagingSrv.PublishAsync(domainEvt);
                    });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    
                });
            });
        }
    }
}