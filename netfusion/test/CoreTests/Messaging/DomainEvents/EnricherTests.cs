using System.Linq;
using System.Threading.Tasks;
using CoreTests.Messaging.DomainEvents.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Test.Container;
using Xunit;
// ReSharper disable All

namespace CoreTests.Messaging.DomainEvents
{
    public class EnricherTests
    {
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

        [Fact]
        public Task Enrichers_Applied_ToMessages()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithDomainEventHandler())
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

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockDomainEventConsumer>();
                    var message = consumer.ReceivedMessages.OfType<MockDomainEvent>().FirstOrDefault();

                    message.Should().NotBeNull();
                    message.GetCorrelationId().Should().NotBeNullOrEmpty();
                    message.GetMessageId().Should().NotBeNullOrEmpty();
                    message.Attributes.GetStringValue("Microservice").Should().NotBeNullOrEmpty();
                    message.Attributes.GetStringValue("MicroserviceId").Should().NotBeNullOrEmpty();
                    message.GetUtcDateOccurred();
                });
            });
        }

        [Fact]
        public void IfEnricherException_ErrorsAreCaptured()
        {
            
        }
    }
}