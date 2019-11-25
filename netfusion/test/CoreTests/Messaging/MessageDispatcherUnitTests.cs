using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetFusion.Messaging;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Types;
using Xunit;

namespace CoreTests.Messaging
{
    /// <summary>
    /// The MessagingService class delegates to an instance of the MessageDispatcher to sent commands and to
    /// publish domain-event messages.  Each message passes through a collection of Message enrichers before
    /// being sent to all registered message publishers.
    /// </summary>
    public class MessageDispatcherUnitTests
    {
        /// <summary>
        /// Message enrichers are called first and are allowed to update existing message properties
        /// or add new properties to the message's dynamic property.
        /// </summary>
        [Fact]
        public async Task AllMessageEnrichersInvoked_BeforeProviders()
        {
            // Arrange
            var enricher = new MessageEnricherOne();
            var publisher = new MessagePublisherOne();

            var dispatcher = SetupDispatcher(config =>
                {
                    config.AddEnricher<MessageEnricherOne>();
                    config.AddPublisher<MessagePublisherOne>();
                }, 
                new[] {enricher},
                new[] {publisher});
                  
            // Act
            var command = new TestCommand();
            await dispatcher.SendAsync(command);
            
            // Assert
            Assert.Equal(2, command.CallSequence.Count);
            Assert.Equal(typeof(MessageEnricherOne), command.CallSequence.ElementAt(0));
            Assert.Equal(typeof(MessagePublisherOne), command.CallSequence.ElementAt(1));
        }

        /// <summary>
        /// If one ore more message enrichers throw an exception, it is captured by the
        /// message dispatch process.  In this case, a PublisherException will be thrown
        /// containing an EnricherException for each failed enricher.  The InnerException
        /// property, of each EnricherException, will reference the actual thrown exception.
        /// A detail JSON document containing the information within the Details property
        /// of the PublisherException is logged.
        /// </summary>
        [Fact]
        public async Task IfEnricherException_ErrorsAreCaptured()
        {
            // Arrange:
            var enricher1 = new MessageEnricherOne
            {
                ExpectedException = new InvalidOperationException("ENRICHER_MESSAGE_1")
            };
            
            var enricher2 = new MessageEnricherOne
            {
                ExpectedException = new InvalidOperationException("ENRICHER_MESSAGE_2")
            };
            
            var publisher = new MessagePublisherOne();

            var dispatcher = SetupDispatcher(config =>
                {
                    config.AddEnricher<MessageEnricherOne>();
                    config.AddEnricher<MessageEnricherTwo>();
                    config.AddPublisher<MessagePublisherOne>();
                }, 
                new[] {enricher1, enricher2},
                new[] {publisher});
                  
            var command = new TestCommand();

            try
            {
                // Act:
                await dispatcher.SendAsync(command);
            }
            catch (PublisherException ex)
            {
                // Assert:
                // ...The publisher exception has a list of exceptions for each failed enricher.
                Assert.NotNull(ex.ExceptionDetails);
                Assert.Equal(2, ex.ExceptionDetails.Count());
                Assert.True(ex.ExceptionDetails.All(dEx => dEx.GetType() == typeof(EnricherException)));
                
                // ... Each enricher exception will have an inner-exception referencing the actual
                // ... exception thrown by the enricher:
                Assert.True(ex.ExceptionDetails.All(dEx => dEx.InnerException?.GetType() == typeof(InvalidOperationException)));
               
                var messages = ex.ExceptionDetails.Select(dEx => dEx.InnerException?.Message).ToArray();
                Assert.Contains("ENRICHER_MESSAGE_1", messages);
                Assert.Contains("ENRICHER_MESSAGE_2", messages);

                // ... If an enricher fails, message publishers are not called.
                Assert.DoesNotContain(command.CallSequence, cs => cs == typeof(MessagePublisher));
            }
        }

        /// <summary>
        /// If one ore more message publishers throw an exception, it is captured by the
        /// message dispatch process.  In this case, a PublisherException will be thrown
        /// containing a PublisherException for each failed publisher.  The InnerException
        /// property, of each PublisherException, will reference the actual thrown exception.
        /// A detail JSON document containing the information within the Details property
        /// of the PublisherException is logged.
        /// </summary>
        [Fact]
        public async Task IfPublisherException_ErrorsAreCaptured()
        {
            // Arrange:
            var enricher = new MessageEnricherOne();

            var publisher1 = new MessagePublisherOne
            {
                ExpectedException = new InvalidOperationException("PUBLISHER_MESSAGE_1")
            };
            
            var publisher2 = new MessagePublisherTwo
            {
                ExpectedException = new InvalidOperationException("PUBLISHER_MESSAGE_2")
            };

            var dispatcher = SetupDispatcher(config =>
                {
                    config.AddEnricher<MessageEnricherOne>();
                    config.AddPublisher<MessagePublisherOne>();
                    config.AddPublisher<MessagePublisherTwo>();
                }, 
                new[] {enricher},
                new IMessagePublisher[] {publisher1, publisher2});
                  
            var command = new TestCommand();

            try
            {
                // Act:
                await dispatcher.SendAsync(command);
            }
            catch (PublisherException ex)
            {
                // Assert:
                // ...The publisher exception has a list of exceptions for each failed enricher.
                Assert.NotNull(ex.ExceptionDetails);
                Assert.Equal(2, ex.ExceptionDetails.Count());
                Assert.True(ex.ExceptionDetails.All(dEx => dEx.GetType() == typeof(PublisherException)));
                
                // ... Each publisher exception will have an inner-exception referencing the actual
                // ... exception thrown by the publisher:
                Assert.True(ex.ExceptionDetails.All(dEx => dEx.InnerException?.GetType() == typeof(InvalidOperationException)));
               
                var messages = ex.ExceptionDetails.Select(dEx => dEx.InnerException?.Message).ToArray();
                Assert.Contains("PUBLISHER_MESSAGE_1", messages);
                Assert.Contains("PUBLISHER_MESSAGE_2", messages);

                // ... If an enricher fails, message publishers are not called.
                Assert.DoesNotContain(command.CallSequence, cs => cs == typeof(MessagePublisher));
            }
        }
        
        private static MessageDispatcher SetupDispatcher(
            Action<MessageDispatchConfig> config, 
            IEnumerable<IMessageEnricher> enrichers, 
            IEnumerable<IMessagePublisher> publishers)
        {
            var dispatchModule = new Mock<IMessageDispatchModule>();
            var dispatchConfig = new MessageDispatchConfig();

            config(dispatchConfig);
            
            dispatchModule.Setup(m => m.DispatchConfig).Returns(dispatchConfig);
            
            return new MessageDispatcher(
                new Mock<ILogger<MessageDispatcher>>().Object, 
                dispatchModule.Object, 
                enrichers, 
                publishers);
        }
        
        //-------------------- TEST ENRICHERS -------------------------
        public abstract class MessageEnricherBase : MessageEnricher
        {          
            public Exception ExpectedException { get; set; }
            
            public override Task Enrich(IMessage message)
            {
                // Return a non-completed task.
                return Task.Run(() =>
                {
                    var command = (TestCommand)message;
                    command.CallSequence.Add(typeof(MessageEnricherOne));
                
                    if (ExpectedException != null) throw ExpectedException;
                    
                });
            }
        }
        
        public class MessageEnricherOne : MessageEnricherBase
        {          
      
        }

        public class MessageEnricherTwo : MessageEnricherBase
        {          
      
        }
        
        //-------------------- TEST PUBLISHERS -------------------------
        public class MessagePublisherBase : MessagePublisher
        {
            public Exception ExpectedException { get; set; }
            
            public override Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
            {
                // Return a non-completed task.
                return Task.Run(() =>
                {
                    var command = (TestCommand)message;
                    command.CallSequence.Add(typeof(MessagePublisherOne));
                    
                    if (ExpectedException != null) throw ExpectedException;
                }, cancellationToken);
            }

            public override IntegrationTypes IntegrationType { get; } = IntegrationTypes.All;
        }

        public class MessagePublisherOne : MessagePublisherBase
        {
            
        }
        
        public class MessagePublisherTwo : MessagePublisherBase
        {
            
        }

        //-------------------- TEST COMMAND -------------------------
        public class TestCommand : Command<int>
        {
            public readonly List<Type> CallSequence = new List<Type>();
        }
    }
}