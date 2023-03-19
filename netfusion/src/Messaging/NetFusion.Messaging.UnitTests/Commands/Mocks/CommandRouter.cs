using NetFusion.Messaging.InProcess;
using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.UnitTests.Commands.Mocks;

// Router used for testing routing commands to consumer query handlers.
public class CommandRouter : MessageRouter
{
    protected override void OnConfigureRoutes() { }

    public void SetupValidSyncCommandRoute()
    {
        OnCommand<TestCommand, TestCommandResult>(
            route => route.ToConsumer<TestCommandConsumer>(c => c.HandleSync));
    }
    public void SetupValidAsyncCommandRoute()
    {
        OnCommand<TestCommand, TestCommandResult>(
            route => route.ToConsumer<TestCommandConsumer>(c => c.HandleAsync));
    }
    
    public void SetupValidAsyncCommandWithCancelRoute()
    {
        OnCommand<TestCommand, TestCommandResult>(
            route => route.ToConsumer<TestCommandConsumer>(c => c.HandleAsyncWithCancel));
    }
    
    public void SetupValidAsyncCommandWithNoResultRoute()
    {
        OnCommand<TestCommand>(
            route => route.ToConsumer<TestCommandConsumer>(c => c.HandleAsyncNoResult));
    }
    
    public void SetupValidSyncCommandWithNoResultRoute()
    {
        OnCommand<TestCommand>(
            route => route.ToConsumer<TestCommandConsumer>(c => c.HandleSyncNoResult));
    }

    public void SetupCommandWithMultipleConsumerHandlers()
    {
        OnCommand<TestCommand, TestCommandResult>(
            route => route.ToConsumer<TestCommandConsumer>(c => c.HandleAsyncWithCancel));
        
        OnCommand<TestCommand, TestCommandResult>(
            route => route.ToConsumer<TestCommandConsumer>(c => c.HandleAsyncWithCancel));
    }

    public class TestCommandResult
    {
        
    }

    public class TestCommand : Command<TestCommandResult>
    {
        
    }

    public class TestCommandConsumer
    {
        public TestCommandResult HandleSync(TestCommand command) => throw new NotImplementedException();
        public Task<TestCommandResult> HandleAsync(TestCommand command) => throw new NotImplementedException();
        
        public Task HandleAsyncNoResult(TestCommand command) => throw new NotImplementedException();
        public void HandleSyncNoResult(TestCommand command) => throw new NotImplementedException();
        
        public Task<TestCommandResult> HandleAsyncWithCancel(TestCommand command, CancellationToken token) 
            => throw new NotImplementedException();
    }
}