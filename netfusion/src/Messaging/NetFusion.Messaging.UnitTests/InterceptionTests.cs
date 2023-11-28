using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging.Test;
using NetFusion.Messaging.Types;

// ReSharper disable PossibleMultipleEnumeration

namespace NetFusion.Messaging.UnitTests;

/// <summary>
/// These tests show how to test a component that sends commands, publishes domain-events, 
/// and dispatches queries by injecting IMessagingService.  Often the component having a
/// dependency on IMessagingService needs to be test with known command and query results.
/// Also, it is often desirable to test if the component interacted with IMessagingService
/// correctly.  This can be accomplished by registering the MockMessagingService class.
/// </summary>
public class InterceptionTests
{
    /// <summary>
    /// Regardless of being code defined in the unit test, this test simulates the
    /// case where a real handler is being called.  Assume the class ExampleAppService
    /// defined below is a handler defined within your application.  The goal is test
    /// this class that has a dependency on IMessagingService with known responses that
    /// can be specified by the unit test.  The service named ExampleAppService below
    /// represents the class we want to unit-test.  The corresponding application
    /// message handler class AppMessageHandler is the class we want to mock.
    /// </summary>
    // [Fact]
    // public Task Simulated_NonMocked_Example()
    // {
    //     return ContainerFixture.TestAsync(async fixture =>
    //     {
    //         CommandResult cmdResult = null;
    //             
    //         var testResult = await fixture
    //             .Arrange.Container(TestSetup.WithEventHandler)
    //             .Act.OnServiceAsync<IExampleAppService>(async service =>
    //             {
    //                 cmdResult = await service.BizLogicWithCommand(30, 50, 77);
    //             });
    //
    //         testResult.Assert.State(() =>
    //         {
    //             cmdResult.Sum.Should().Be(257);
    //         });
    //     });
    // }
        
    /// <summary>
    /// In order to test AppMessageHandler with known responses, the unit-test class named
    /// ExampleAppService will be register for IMessagingService.  This class allows us to
    /// specified known responses to commands and queries issued by the ServiceUnderTest.
    /// The code being tested is contained within the ExampleAppService which in this case
    /// is just a simple expression.  Below shows how MockMessagingService is registered
    /// and populated with a known response.  The AppMessageHandler being replaced with
    /// known responses could be calling the database or REST Api.
    /// </summary>
    [Fact]
    public Task ResponsesToCommands_CanBeRegistered()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            CommandResult? cmdResult = null;
                
            var mockMsgSrv = new MockMessagingService();
            mockMsgSrv.AddCommandResponse<ExampleCommand>(new CommandResult { Sum = 100, Avg = 88 });
                
            var testResult = await fixture.Arrange
                .Services(services => services.AddScoped<IMessagingService>(_ => mockMsgSrv))
                .Container(TestSetup.WithEventHandler)
                    
                .Act.OnServiceAsync<IExampleAppService>(async service =>
                {
                    cmdResult = await service.BizLogicWithCommand(30, 50, 77);
                });

            testResult.Assert.State(() =>
            {
                Assert.NotNull(cmdResult);
                cmdResult.Sum.Should().Be(200);
                cmdResult.Avg.Should().Be(138);
            });
        });
    }

    /// <summary>
    /// The following shows the case where we want to unit-test the state of the command being
    /// sent to the registered handler.  As in the previous example, the mock implementation of
    /// the IMessageService can be used.
    /// </summary>
    [Fact]
    public Task ReceivedCommands_CanBeAsserted()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var mockMsgSrv = new MockMessagingService();
            mockMsgSrv.AddCommandResponse<ExampleCommand>(new CommandResult { Sum = 100, Avg = 88 });
                
            var testResult = await fixture.Arrange
                .Services(services => services.AddScoped<IMessagingService>(_ => mockMsgSrv))
                .Container(TestSetup.WithEventHandler)
                    
                .Act.OnServiceAsync<IExampleAppService>(async service =>
                {
                    await service.BizLogicWithCommand(30, 50, 77);
                });

            testResult.Assert.State(() =>
            {
                var commands = mockMsgSrv.GetReceivedCommands<ExampleCommand>();
                commands.Should().HaveCount(1);
                commands.First().Values.Should().BeEquivalentTo(new[] {30, 50, 77});
            });
        });
    }
        
    /// <summary>
    /// As for commands, known responses to queries can be registered.
    /// </summary>
    [Fact]
    public Task ResponsesToQueries_CanBeRegistered()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            QueryResult? queryResult = null;
                
            var mockMsgSrv = new MockMessagingService();
            mockMsgSrv.AddQueryResponse<ExampleQuery>(new QueryResult { Data = new [] { 100, 200 }});
                
            var testResult = await fixture.Arrange
                .Services(services => services.AddScoped<IMessagingService>(_ => mockMsgSrv))
                .Container(TestSetup.WithEventHandler)
                    
                .Act.OnServiceAsync<IExampleAppService>(async service =>
                {
                    queryResult = await service.BizLogicWithQuery(10);
                });

            testResult.Assert.State(() =>
            {
                Assert.NotNull(queryResult);
                queryResult.Data.Should().NotBeNull();
                queryResult.Data.Should().BeEquivalentTo(new[] {100, 200, 99});
            });
        });
    }
        
    /// <summary>
    /// Queries dispatched by a business service component can be asserted.
    /// </summary>
    [Fact]
    public Task ReceivedQueries_CanBeAsserted()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var mockMsgSrv = new MockMessagingService();
            mockMsgSrv.AddQueryResponse<ExampleQuery>(new QueryResult { Data = new [] { 100, 200 }});
                
            var testResult = await fixture.Arrange
                .Services(services => services.AddScoped<IMessagingService>(_ => mockMsgSrv))
                .Container(TestSetup.WithEventHandler)
                    
                .Act.OnServiceAsync<IExampleAppService>(async service =>
                {
                    await service.BizLogicWithQuery(10);
                });

            testResult.Assert.State(() =>
            {
                var queries = mockMsgSrv.GetReceivedQueries<ExampleQuery>();
                queries.Should().HaveCount(1);
                queries.First().MinValue.Should().Be(10);
            });
        });
    }
        
    /// <summary>
    /// Domain Events published by a business service component can be asserted.
    /// </summary>
    [Fact]
    public Task ReceivedDomainEvents_CanBeAsserted()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var mockMsgSrv = new MockMessagingService();

            var testResult = await fixture.Arrange
                .Services(services => services.AddScoped<IMessagingService>(_ => mockMsgSrv))
                .Container(TestSetup.WithEventHandler)
                    
                .Act.OnServiceAsync<IExampleAppService>(async service =>
                {
                    await service.BizLogicWithDomainEvent("Unit-Test-Value");
                });

            testResult.Assert.State(() =>
            {
                var domainEvents = mockMsgSrv.GetReceivedDomainEvents<ExampleDomainEvent>();
                domainEvents.Should().HaveCount(1);
                domainEvents.First().State.Should().Be("Unit-Test-Value");
            });
        });
    }
        
    [Fact]
    public Task ExceptionToCommand_CanBeRegistered()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var mockMsgSrv = new MockMessagingService();
            mockMsgSrv.AddCommandResponse<ExampleCommand>(new InvalidOperationException("Error handling command."));
                
            var testResult = await fixture.Arrange
                .Services(services => services.AddScoped<IMessagingService>(_ => mockMsgSrv))
                .Container(TestSetup.WithEventHandler)
                    
                .Act.RecordException().OnServiceAsync<IExampleAppService>(async service =>
                {
                    await service.BizLogicWithCommand(30, 50, 77);
                });

            testResult.Assert.Exception<InvalidOperationException>(ex =>
                ex.Message.Should().Be("Error handling command."));
        });
    }

    [Fact]
    public Task ExceptionToQueries_CanBeRegistered()
    {
        return ContainerFixture.TestAsync(async fixture =>
        {
            var mockMsgSrv = new MockMessagingService();
            mockMsgSrv.AddQueryResponse<ExampleQuery>(new InvalidOperationException("Error handling query."));
                
            var testResult = await fixture.Arrange
                .Services(services => services.AddScoped<IMessagingService>(_ => mockMsgSrv))
                .Container(TestSetup.WithEventHandler)
                    
                .Act.RecordException().OnServiceAsync<IExampleAppService>(async service =>
                {
                    await service.BizLogicWithQuery(30);
                });

            testResult.Assert.Exception<InvalidOperationException>(ex =>
                ex.Message.Should().Be("Error handling query."));
        });
    }
        
        
    //-- Unit test classes:

    private interface IExampleAppService
    {
        Task<CommandResult> BizLogicWithCommand(params int[] values);
        Task<QueryResult> BizLogicWithQuery(int minValue);
        Task BizLogicWithDomainEvent(string state);
    }

    private class ExampleAppService : IExampleAppService
    {
        private readonly IMessagingService _messaging;
            
        public ExampleAppService(IMessagingService messaging)
        {
            _messaging = messaging;
        }

        public async Task<CommandResult> BizLogicWithCommand(params int[] values)
        {
            var cmd = new ExampleCommand(values);
            var result = await _messaging.SendAsync(cmd);

            result.Sum += 100;
            result.Avg += 50;
                
            return result;
        }

        public async Task<QueryResult> BizLogicWithQuery(int minValue)
        {
            var query = new ExampleQuery(minValue);
            var result = await _messaging.ExecuteAsync(query);

            var baseResult = result.Data.ToList();
            baseResult.Add(99);
            result.Data = baseResult.ToArray();

            return result;
        }

        public Task BizLogicWithDomainEvent(string state)
        {
            var domainEvt = new ExampleDomainEvent(state);
            return _messaging.PublishAsync(domainEvt);
        }
    }

    public class ExampleModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IExampleAppService, ExampleAppService>();
        }
    }

    public class ExampleDomainEvent : DomainEvent
    {
        public string State { get; }
            
        public ExampleDomainEvent(string state)
        {
            State = state;
        }
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
        public int[] Data { get; set; } = Array.Empty<int>();
    }

    public class ExampleQuery : Query<QueryResult>
    {
        public int MinValue { get; }

        public ExampleQuery(int minValue)
        {
            MinValue = minValue;
        }
    }
        
    public class AppMessageHandler
    {
        public CommandResult OnCommand(ExampleCommand command)
        {
            return new CommandResult
            {
                Sum = command.Values.Sum(),
                Avg = command.Values.Average()
            };
        }
            
        public QueryResult OnQuery(ExampleQuery query)
        {
            return new QueryResult
            {
                Data = new[] { 100, 200, 400, 330 }.Where(v => v >= query.MinValue).ToArray()
            };
        }
            
        public void OnDomainEvent(ExampleDomainEvent domainEvt)
        {
                
        }
    }

}