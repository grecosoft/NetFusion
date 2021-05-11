using System;
using System.Linq;
using System.Threading.Tasks;
using CoreTests.Messaging.DomainEvents;
using CoreTests.Messaging.DomainEvents.Mocks;
using CoreTests.Messaging.Enrichers.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Test.Container;
using Xunit;
// ReSharper disable All

namespace CoreTests.Messaging.Enrichers
{
    /// <summary>
    /// Configured message enrichers are invoked and applied to command and domain-event messages
    /// before they are dispatched.
    /// </summary>
    public class EnricherTests
    {
        /// <summary>
        /// By default, NetFusion configures a set of common enrichers.
        /// </summary>
        [Fact]
        public void DefaultEnrichers_Configured()
        {
            ContainerFixture.Test(fixture => { 
                fixture
                    .Arrange.Container(c => c.AddMessagingHost())                
                    .Assert.Configuration<MessageDispatchConfig>(c =>
                    {
                        c.EnricherTypes.Should().NotBeNull();
                        c.EnricherTypes.Should().HaveCount(3);
                        c.EnricherTypes.Should().Contain(typeof(CorrelationEnricher));
                        c.EnricherTypes.Should().Contain(typeof(DateOccurredEnricher));
                        c.EnricherTypes.Should().Contain(typeof(HostEnricher));
                    });
            });
        }

        /// <summary>
        /// During the bootstrapping of the composite-application, the host can clear all default
        /// registered enrichers and replace with existing or custom ones.
        /// </summary>
        [Fact]
        public void DefaultEnrichers_CanBe_Cleared()
        {
            ContainerFixture.Test(fixture => { 
                fixture
                    .Arrange.Container(c => c.AddMessagingHost())  
                    .PluginConfig<MessageDispatchConfig>(c =>
                    {
                        c.ClearEnrichers();
                        c.AddEnricher<CorrelationEnricher>();
                    })
                    .Assert.Configuration<MessageDispatchConfig>(c =>
                    {
                        c.EnricherTypes.Should().HaveCount(1);
                        c.EnricherTypes.Should().Contain(typeof(CorrelationEnricher));
                    });
            });
        }

        /// <summary>
        /// Before a message is sent to all registered IMessagePublisher instances, each enricher is applied
        /// to the message allowing common message attributed to be set.
        /// </summary>
        [Fact]
        public Task Enrichers_Applied_ToMessages()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithSyncDomainEventHandler())
                    .PluginConfig<MessageDispatchConfig>(c =>
                    {
                        c.ClearEnrichers();
                        c.AddEnricher<CorrelationEnricher>();
                        c.AddEnricher<HostEnricher>();
                        c.AddEnricher<DateOccurredEnricher>();
                    })
                    .Act.OnServicesAsync(async s =>
                    {
                        var mockEvt = new MockDomainEvent();
                        await s.GetRequiredService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    var message = log.Messages.OfType<MockDomainEvent>().FirstOrDefault();

                    // Correlation Enricher:
                    message.Should().NotBeNull();
                    message.GetCorrelationId().Should().NotBeNullOrEmpty();
                    message.GetMessageId().Should().NotBeNullOrEmpty();
                    
                    // Microservice Enricher:
                    message.Attributes.GetStringValue("Microservice").Should().NotBeNullOrEmpty();
                    message.Attributes.GetStringValue("MicroserviceId").Should().NotBeNullOrEmpty();
                    
                    // Date Occurred Enricher:
                    message.GetUtcDateOccurred();
                });
            });
        }

        /// <summary>
        /// Validates when an enricher throws and exception, all details are correctly captured.
        /// </summary>
        [Fact]
        public Task IfEnricherException_ErrorsAreCaptured()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c =>
                    {
                        c.AddMessagingHost();
                        c.WithSyncDomainEventHandler();
                    })
                    .PluginConfig<MessageDispatchConfig>(dc =>
                    {
                        dc.AddEnricher<MockEnricherWithException>();
                    })
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var domainEvt = new MockDomainEvent();

                        await messagingSrv.PublishAsync(domainEvt);
                    });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    ex.ChildExceptions.Should().HaveCount(1);
                    var childEx = ex.ChildExceptions.First();

                    childEx.Should().NotBeNull();
                    childEx.Should().BeOfType<EnricherException>();
                    childEx.InnerException.Should().BeOfType<InvalidOperationException>();
                    childEx.InnerException.Message.Should().Be("TestEnricherException");
                });
            });
        }
    }
}