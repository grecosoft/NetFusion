using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.UnitTests.Queries.Mocks;

public class QueryFilterOne : IPreMessageFilter, IPostMessageFilter
{
    public Task OnPreFilterAsync(IMessage message)
    {
        if (message is MockQuery testQuery)
        {
            testQuery.QueryAsserts.Add("QueryFilterOne-Pre");
        }

        return Task.CompletedTask;
    }

    public Task OnPostFilterAsync(IMessage message)
    {
        if (message is MockQuery testQuery)
        {
            if (testQuery.ThrowInHandlers.Contains(nameof(QueryFilterOne)))
            {
                return Task.Run(() => throw new InvalidOperationException("TestQueryFilterException"));
            }

            testQuery.QueryAsserts.Add("QueryFilterOne-Post");
        }

        return Task.CompletedTask;
    }
}

public class QueryFilterTwo : IPreMessageFilter, IPostMessageFilter
{
    public Task OnPreFilterAsync(IMessage message)
    {
        if(message is MockQuery testQuery)
        { 
            testQuery.QueryAsserts.Add("QueryFilterTwo-Pre");
        }

        return Task.CompletedTask;
    }

    public Task OnPostFilterAsync(IMessage message)
    {
        if (message is MockQuery testQuery)
        { 
            testQuery.QueryAsserts.Add("QueryFilterTwo-Post");
        }

        return Task.CompletedTask;
    }
}