using NetFusion.Messaging.InProcess;
using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.UnitTests.Queries.Mocks;

// Router used for testing routing queries to consumer query handlers.
public class QueryRouter : MessageRouter
{
    protected override void OnConfigureRoutes() { }

    public void SetupValidSyncQueryRoute()
    {
        OnQuery<TestQuery, TestQueryResult>(
            route => route.ToConsumer<TestQueryConsumer>(c => c.HandleSync));
    }
    public void SetupValidAsyncQueryRoute()
    {
        OnQuery<TestQuery, TestQueryResult>(
            route => route.ToConsumer<TestQueryConsumer>(c => c.HandleAsync));
    }
    
    public void SetupValidAsyncQueryWithCancelRoute()
    {
        OnQuery<TestQuery, TestQueryResult>(
            route => route.ToConsumer<TestQueryConsumer>(c => c.HandleAsyncWithCancel));
    }

    public void SetupQueryWithMultipleConsumerHandlers()
    {
        OnQuery<TestQuery, TestQueryResult>(
            route => route.ToConsumer<TestQueryConsumer>(c => c.HandleAsyncWithCancel));
        
        OnQuery<TestQuery, TestQueryResult>(
            route => route.ToConsumer<TestQueryConsumer>(c => c.HandleSync));
    }

    public class TestQueryResult
    {

    }

    public class TestQuery : Query<TestQueryResult>
    {
        
    }

    public class TestQueryConsumer
    {
        public TestQueryResult HandleSync(TestQuery query) => throw new NotImplementedException();
        public Task<TestQueryResult> HandleAsync(TestQuery query) => throw new NotImplementedException();
        public Task<TestQueryResult> HandleAsyncWithCancel(TestQuery query, CancellationToken token) => throw new NotImplementedException();
    }
}