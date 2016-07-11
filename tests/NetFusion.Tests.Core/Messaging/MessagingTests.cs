using Autofac;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Testing;
using NetFusion.Core.Tests.Messaging.Mocks;
using NetFusion.Messaging;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.Tests.Core.Bootstrap.Mocks;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Eventing
{
    /// <summary>
    /// Domain events provide a way of modeling events that exist within the problem
    /// domain and needed to be handled by one or more application components.  Domain
    /// events also provide a means of keeping application components loosely coupled.
    /// Domain events are usually in-process and single threaded.  However, asynchronous
    /// non-blocking event handling is also often provided.
    /// 
    /// The domain-event plug-in provides an extension point where other plug-ins can 
    /// extend this behavior and provide the delivering of events on a service bus.
    /// </summary>
    public class MessagingTests
    {
        /// <summary>
        /// The domain event plug-in has a configuration class for specifying related settings.
        /// </summary>
        [Fact]
        public void DiscoversMessagingConfiguration()
        {
            DefaultDomainEventPlugin
                .Act(c =>
                {
                    c.WithConfig<MessagingConfig>(config => config.ConsumerMethodPrefix = "WhenEvent");
                    c.Build();
                })
                .Assert((MessagingModule m) =>
                {
                    var moduleConfig = m.Context.Plugin.GetConfig<MessagingConfig>();
                    moduleConfig.Should().NotBeNull();
                    moduleConfig.ConsumerMethodPrefix.Should().Be("WhenEvent");
                });
        }

        /// <summary>
        /// The domain event module will discover all defined domain event classes
        /// within all available plug-ins.  The module's EventTypeDispatchers property
        /// contains all of the meta-data required to dispatch an event at runtime to
        /// the correct consumer handlers.
        /// </summary>
        [Fact]
        public void DiscoversAllDomainEvents()
        {
            DefaultDomainEventPlugin
                .Act(c => c.Build())
                .Assert((MessagingModule m) =>
                {
                    m.InProcessMessageTypeDispatchers
                        .Count(et => et.Key == typeof(MockDomainEvent))
                        .Should().Be(1);
                });
        }

        /// <summary>
        /// The domain event module will discover all defined domain event consumer classes 
        /// with methods that know how to handle a given domain event.  Application components
        /// can be scanned for event handler methods during the bootstrap process by implementing
        /// the IDomainEventConsumer marker interface.
        /// </summary>
        [Fact]
        public void DiscoversDomainEventConsumerHandler()
        {
            DefaultDomainEventPlugin
                .Act(c => c.Build())
                .Assert((MessagingModule m) =>
                {
                    var eventDispatchers = m.InProcessMessageTypeDispatchers[typeof(MockDomainEvent)];
                    eventDispatchers.Should().HaveCount(1);

                    var dispatchInfo = eventDispatchers.First();
                    dispatchInfo.MessageType.Should().Equals(typeof(MockDomainEvent));
                    dispatchInfo.ConsumerType.Should().Equals(typeof(MockDomainEventConsumer));
                    dispatchInfo.MessageHandlerMethod.Should()
                        .Equals(typeof(MockDomainEventConsumer).GetMethod("OnEventHandlerOne"));
                });
        }

        /// <summary>
        /// The plug-n registers a service that can be used to publish events.
        /// </summary>
        [Fact]
        public void RegistersServiceForPublishingEvents()
        {
            DefaultDomainEventPlugin
                .Act(c => c.Build())
                .Assert((AppContainer c) =>
                {
                    var service = c.Services.Resolve<IMessagingService>();
                    service.Should().NotBeNull();
                });
        }

        /// <summary>
        /// All discovered event consumers are registered in the dependency injection container.
        /// When an event is published, the domain event service resolves the consumer type
        /// and obtains a reference from the container using the cached dispatch information.
        /// </summary>
        [Fact]
        public void AllDomainEventConsumersRegistered()
        {
            DefaultDomainEventPlugin
               .Act(c => c.Build())
               .Assert((AppContainer c) =>
               {
                   var consumer = c.Services.Resolve<MockDomainEventConsumer>();
                   consumer.Should().NotBeNull();
               });
        }

        /// <summary>
        /// When a domain event is published using the service, the corresponding discovered
        /// consumer event handler methods will be invoked.
        /// </summary>
        [Fact]
        public void DomainEventConsumerHandlerInvoked()
        {
            DefaultDomainEventPlugin
                .Act(c =>
                {
                    c.Build();

                    var mockEvt = new MockDomainEvent();
                    var futureResponse = c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);

                    futureResponse.Wait();

                })
                .Assert((AppContainer c) =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockDomainEventConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().ContainSingle("OnEventHandlerOne");
                });
        }

        /// <summary>
        /// The hosting application can specify a prefix to indicate domain event handler methods.  
        /// If this is specified, only methods starting with the prefix and have the corresponding 
        /// domain event type as a parameter will be invoked.
        /// </summary>
        [Fact]
        public void AppHostCanSpecifyMessageHandlerMethodNamePrefix()
        {
            DefaultPrefixedDomainEventPlugin
                 .Act(c =>
                 {
                     c.WithConfig<MessagingConfig>(config => config.ConsumerMethodPrefix = "WhenEvent");
                     c.Build();
                 })
                .Assert((MessagingModule m) =>
                {
                    var eventDispatchers = m.InProcessMessageTypeDispatchers[typeof(MockDomainEvent)];
                    eventDispatchers.Should().HaveCount(1);

                    var dispatchInfo = eventDispatchers.First();
                    dispatchInfo.MessageType.Should().Equals(typeof(MockDomainEvent));
                    dispatchInfo.ConsumerType.Should().Equals(typeof(MockDomainEventConsumer));
                    dispatchInfo.MessageHandlerMethod.Should().Equals(typeof(MockDomainEventConsumer).GetMethod("WhenEventHandlerTwo"));
                });
        }

        /// <summary>
        /// By default, consumer event handlers with a base domain event type
        /// will not be invoked for derived events.
        /// </summary>
        [Fact]
        public void ConsumerEventHandlerForBaseTypeNotInvokedByDefault()
        {
            DefaultDerivedDomainEventPlugin
                .Act(c =>
                {
                    var mockEvt = new MockDerivedDomainEvent();

                    // Note:  specifying the prefix so the event handler that is 
                    // specified to include base event types will not be called.
                    c.WithConfig<MessagingConfig>(config => config.ConsumerMethodPrefix = "OnBaseEvent");
                    c.Build();

                    var futureResponse = c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);

                    futureResponse.Wait();

                })
                .Assert((AppContainer c) =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockBaseMessageConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().BeEmpty();
                });
        }

        /// <summary>
        /// A consumer event handler for a base domain event type can be marked
        /// with the IncludeDerivedEvents attribute to indicate it should be 
        /// called for any derived domain events.
        /// </summary>
        [Fact]
        public void ConsumerEventHandlerForBaseTypeInvokedIfAppliedAttribute()
        {
            DefaultDerivedDomainEventPlugin
                .Act(c =>
                {

                    c.Build();

                    var mockEvt = new MockDerivedDomainEvent();
                    var futureResponse = c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);

                    futureResponse.Wait();

                })
                .Assert((AppContainer c) =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockBaseMessageConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
        }

        /// <summary>
        /// Unless cleared, the InProcessEventDispatcher will be used by default
        /// to dispatch events to local consumer event handlers.  
        /// </summary>
        [Fact]
        public void InProcessEventDispatcherConfiguredByDefault()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MessagingConfig>()
                        .AddPluginType<MessagingModule>();
                })
                .Act(c =>
                {
                    c.WithConfig<MessagingConfig>();
                    c.Build();

                })
                .AssertConfig((MessagingConfig ca) =>
                {
                    ca.PublisherTypes.Should().HaveCount(1);
                    ca.PublisherTypes.Should().Contain(typeof(InProcessMessagePublisher));
                });
        }

        /// <summary>
        /// If the host application doesn't want to have the default InProcessEventDispatcher
        /// invoked, they can clear the list and add the desired dispatcher types.
        /// </summary>
        [Fact]
        public void DefaultDispatcherCanBeCleared()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>();
                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MessagingConfig>()
                        .AddPluginType<MessagingModule>();
                })
                .Act(c =>
                {
                    c.WithConfig<MessagingConfig>(config => config.ClearMessagePublishers());
                    c.Build();

                })
                .AssertConfig((MessagingConfig ca) =>
                {
                    ca.PublisherTypes.Should().HaveCount(0);
                });
        }

        [Fact]
        public void ExceptionsRecordedForEachEventHandler()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.AddPlugin<MockAppHostPlugin>()
                        .AddPluginType<MockDomainEvent>()
                        .AddPluginType<MockErrorMessageConsumer>();

                    config.AddPlugin<MockCorePlugin>()
                        .AddPluginType<MessagingConfig>()
                        .AddPluginType<MessagingModule>();
                })
                .Act(c =>
                {
                    c.Build();

                    var srv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockDomainEvent();
                    var futureResult = srv.PublishAsync(evt);
                    futureResult.Wait();
                })
                .Assert((c, e) =>
                {

                });
        }

        [Fact]
        public void DispatchRuleDeterminesIfHandlerIsCalledForEvent()
        {

        }

        /// <summary>
        /// A command domain event can have only one event consumer handler.
        /// If there are more than one event handler, an exception is raised.
        /// </summary>
        [Fact]
        public void CommandEventsCanOnlyHaveOneEventHandler()
        {
            DefaultCommandDomainEventPlugin
                .Act(c =>
                {
                    c.WithConfig<MessagingConfig>(config => config.ConsumerMethodPrefix = "Invalid");
                    c.Build();
                })
                .Assert((c, e) =>
                {
                    e.Should().NotBeNull();
                    e.Should().BeOfType<ContainerException>();
                });
        }

        /// <summary>
        /// Command domain events can have a return result. 
        /// </summary>
        [Fact]
        public void CommandEventsCanHaveResult()
        {
            MockCommandResult result = null;

            DefaultCommandDomainEventPlugin
                .Act(c =>
                {
                    c.WithConfig<MessagingConfig>(config => config.ConsumerMethodPrefix = "On");
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockCommand();
                    var futureResult = domainEventSrv.PublishAsync(evt);

                    result = futureResult.Result;

                })
                .Assert((AppContainer c) =>
                {
                    result.Should().NotBeNull();
                    var consumer = c.Services.Resolve<MockCommandConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(1);
                    consumer.ExecutedHandlers.Should().Contain("OnCommand");
                });
        }

        /// <summary>
        /// When a domain-event is published, the event handlers can be
        /// asynchronous.  This test publishes an event that will be
        /// handled by two asynchronous handlers and one synchronous.
        /// </summary>
        [Fact]
        public void AsyncHandlersCanBeInvoked()
        {
            DefaultAsyncDomainEventPlugin
                .Act(c =>
                {
                    c.WithConfig<MessagingConfig>();
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockDomainEvent();
                    var futureResults = domainEventSrv.PublishAsync(evt);

                    futureResults.Wait();

                })
                .Assert((AppContainer c) =>
                {
                    var consumer = c.Services.Resolve<MockAsyncMessageConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(3);
                    consumer.ExecutedHandlers.Should().Contain("OnEvent1");
                    consumer.ExecutedHandlers.Should().Contain("OnEvent2");
                    consumer.ExecutedHandlers.Should().Contain("OnEvent3");
                });
        }

        private ContainerAct DefaultDomainEventPlugin
        {
            get
            {
                return ContainerSetup
                   .Arrange((TestTypeResolver config) =>
                   {
                       // Use the application host to simulate a plug-in with domain-event related 
                       // types that will be discovered by the Domain Event Plug-in.
                       config.AddPlugin<MockAppHostPlugin>()
                            .AddPluginType<MockDomainEvent>()
                            .AddPluginType<MockDomainEventConsumer>();

                       config.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MessagingConfig>()
                            .AddPluginType<MessagingModule>();
                   });
            }
        }

        private ContainerAct DefaultAsyncDomainEventPlugin
        {
            get
            {
                return ContainerSetup
                   .Arrange((TestTypeResolver config) =>
                   {
                       // Use the application host to simulate a plug-in with domain-event related 
                       // types that will be discovered by the Domain Event Plug-in.
                       config.AddPlugin<MockAppHostPlugin>()
                            .AddPluginType<MockDomainEvent>()
                            .AddPluginType<MockAsyncMessageConsumer>();

                       config.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MessagingConfig>()
                            .AddPluginType<MessagingModule>();
                   });
            }
        }

        private ContainerAct DefaultCommandDomainEventPlugin
        {
            get
            {
                return ContainerSetup
                   .Arrange((TestTypeResolver config) =>
                   {
                       // Use the application host to simulate a plug-in with domain-event related 
                       // types that will be discovered by the Domain Event Plug-in.
                       config.AddPlugin<MockAppHostPlugin>()
                            .AddPluginType<MockCommand>()
                            .AddPluginType<MockCommandConsumer>();

                       config.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MessagingConfig>()
                            .AddPluginType<MessagingModule>();
                   });
            }
        }

        private ContainerAct DefaultPrefixedDomainEventPlugin
        {
            get
            {
                return ContainerSetup
                   .Arrange((TestTypeResolver config) =>
                   {
                       // Use the application host to simulate a plug-in with domain-event related 
                       // types that will be discovered by the Domain Event Plug-in.
                       config.AddPlugin<MockAppHostPlugin>()
                            .AddPluginType<MockDomainEvent>()
                            .AddPluginType<MockPrefixedMessageConsumer>();

                       config.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MessagingConfig>() // TODO: See why test fails if this is removed..
                            .AddPluginType<MessagingModule>();
                   });
            }
        }

        private ContainerAct DefaultDerivedDomainEventPlugin
        {
            get
            {
                return ContainerSetup
                   .Arrange((TestTypeResolver config) =>
                   {
                       // Use the application host to simulate a plug-in with domain-event related 
                       // types that will be discovered by the Domain Event Plug-in.
                       config.AddPlugin<MockAppHostPlugin>()
                            .AddPluginType<MockDerivedDomainEvent>()
                            .AddPluginType<MockBaseMessageConsumer>();

                       config.AddPlugin<MockCorePlugin>()
                            .AddPluginType<MessagingConfig>()
                            .AddPluginType<MessagingModule>();

                       var corePlugin = new MockCorePlugin();
                   });
            }
        }
    }
}
