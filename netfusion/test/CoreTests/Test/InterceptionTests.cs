using System.Linq;
using System.Threading.Tasks;
using CoreTests.Test.Setup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CoreTests.Test
{
    /// <summary>
    /// These tests show how to test a component that sends commands, publishes domain-events, 
    /// and dispatches queries by injecting IMessagingService.  Often the component having a
    /// dependency on IMessagingService needs to be test with known command and query results.
    /// Also, it is often desirable to test if the component interacted with IMessagingService
    /// correctly.  This can be accomplished by registering the MockMessagingService class.
    /// </summary>
    public class InterceptionTests
    {
        [Fact]
        public Task ReceivedCommands_CanBeAsserted()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                CommandResult cmdResult = null;
                
                var testResult = await fixture
                    .Arrange.Container(TestSetup.WithEventHandler)
                    .Act.OnServiceAsync<IServiceUnderTest>(async service =>
                    {
                        cmdResult = await service.SendCommand(30, 50, 77);
                    });

                testResult.Assert.State(() =>
                {
                    cmdResult.Sum.Should().Be(157);
                });
            });
        }

        [Fact]
        public void ReceivedDomainEvents_CanBeAsserted()
        {
            
        }

        [Fact]
        public void ReceivedQueries_CanBeAsserted()
        {
            
        }

        [Fact]
        public void ResponsesToCommands_CanBeRegistered()
        {
            
        }

        [Fact]
        public void ResponsesToQueries_CanBeRegistered()
        {
            
        }

        [Fact]
        public void ExceptionToCommand_CanBeRegistered()
        {
            
        }

        [Fact]
        public void ExceptionToQueries_CanBeRegistered()
        {
            
        }
        
        
        //-- Unit test classes:

        private interface IServiceUnderTest
        {
            Task<CommandResult> SendCommand(params int[] values);
        }

        private class ServiceUnderTest : IServiceUnderTest
        {
            private readonly IMessagingService _messaging;
            
            public ServiceUnderTest(IMessagingService messaging)
            {
                _messaging = messaging;
            }

            public Task<CommandResult> SendCommand(params int[] values)
            {
                var cmd = new ExampleCommand(values);
                return _messaging.SendAsync(cmd);
            }
        }

        public class ExampleModule : PluginModule
        {
            public override void RegisterServices(IServiceCollection services)
            {
                services.AddScoped<IServiceUnderTest, ServiceUnderTest>();
            }
        }

        public class ExampleDomainEvent : DomainEvent
        {
            
        }

        public class CommandResult
        {
            public int Sum { get; set; }
            public double Avg { get; set; }
        }

        public class ExampleCommand : Command<CommandResult>
        {
            public int[] Values { get; }
            
            public ExampleCommand(int[] values)
            {
                Values = values;
            }
        }

        public class QueryResult
        {
            
        }

        public class ExampleQuery : Query<QueryResult>
        {
            
        }
        
        public class RealMessageHandler : IMessageConsumer
        {
            [InProcessHandler]
            public CommandResult OnCommand(ExampleCommand command)
            {
                return new CommandResult
                {
                    Sum = command.Values.Sum(),
                    Avg = command.Values.Average()
                };
            }
        }

    }
}