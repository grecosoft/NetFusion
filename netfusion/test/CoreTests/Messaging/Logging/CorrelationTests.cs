using System.Linq;
using System.Threading.Tasks;
using CoreTests.Messaging.DomainEvents;
using CoreTests.Messaging.DomainEvents.Mocks;
using CoreTests.Messaging.Logging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Logging;
using NetFusion.Test.Container;
using Xunit;

// ReSharper disable All

namespace CoreTests.Messaging.Logging
{
    /// <summary>
    /// When a message is published, the IMessageLogger is used to log information about
    /// sent and received messages.  This logging is additional to the more detailed logs
    /// emitted by ILogger.  This logger allows external consumers such as developer tools
    /// for monitoring messages.  This should only be used during development or researching
    /// a production issue.
    /// </summary>
    public class CorrelationTests
    {
        [Fact]
        public void MessageSinks_CanBeRegistered()
        {
            ContainerFixture.Test(fixture => { 
                fixture
                    .Arrange.Services(s => s.AddMessageLogSink<MockLoggingSink>())
                    .Container(c => c.AddMessagingHost())                
                    .Assert.ServiceCollection(sc =>
                    {
                        var registration = sc.FirstOrDefault(s => s.ServiceType == typeof(IMessageLogSink));
                        registration.Lifetime.Should().Be(ServiceLifetime.Singleton);
                    })
                    .Services(s =>
                    {
                        var services = s.GetServices<IMessageLogSink>();
                        services.Should().ContainSingle();
                    });
            });
        }

        [Fact]
        public Task LogMessage_DeliveredTo_AllSinks()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Services(sc => sc.AddMessageLogSink<MockLoggingSink>())
                    .Container(c => c.AddMessagingHost().WithSyncDomainEventHandler())
                    .Act.OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var domainEvt = new MockDomainEvent();

                        await messagingSrv.PublishAsync(domainEvt);
                    });

                testResult.Assert.Service<IMessageLogSink>(srv =>
                {
                    var sink = (MockLoggingSink)srv;
                    var log = sink.ReceivedLogs.First();
                    
                    log.Message.Should().BeOfType<MockDomainEvent>();
                    log.MessageType.Should().Be(typeof(MockDomainEvent).FullName);
                    log.LogContext.Should().Be(LogContextType.PublishedMessage);
                    log.LogDetails.Should().NotBeEmpty();
                    log.LogErrors.Should().BeEmpty();
                    log.Hint.Should().Be("publish-in-process");
                });
            });
        }

        [Fact]
        public Task LogMessageErrors_DeliveredTo_AllSinks()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Services(sc => sc.AddMessageLogSink<MockLoggingSink>())
                    .Container(c => c.AddMessagingHost().WithMessageHandlerException())
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var domainEvt = new MockDomainEvent();

                        await messagingSrv.PublishAsync(domainEvt);
                    });

                testResult.Assert.Service<IMessageLogSink>(srv =>
                {
                    var sink = (MockLoggingSink)srv;
                    var log = sink.ReceivedLogs.First();
                    
                    log.Message.Should().BeOfType<MockDomainEvent>();
                    log.MessageType.Should().Be(typeof(MockDomainEvent).FullName);
                    log.LogContext.Should().Be(LogContextType.PublishedMessage);
                    log.LogDetails.Should().NotBeEmpty();
                    log.LogErrors.Should().NotBeEmpty();
                    log.Hint.Should().Be("publish-in-process");
                });
            });
        }
    }
}