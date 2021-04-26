using System;
using System.Threading.Tasks;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Types.Contracts;

namespace CoreTests.Messaging.Queries.Mocks
{
    public class QueryFilterOne : IPreQueryFilter, IPostQueryFilter
    {
        public Task OnPreExecuteAsync(IQuery query)
        {
            if (query is MockQuery testQuery)
            {
                testQuery.QueryAsserts.Add("QueryFilterOne-Pre");
            }

            return Task.CompletedTask;
        }

        public Task OnPostExecuteAsync(IQuery query)
        {
            if (query is MockQuery testQuery)
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

    public class QueryFilterTwo : IPreQueryFilter, IPostQueryFilter
    {
        public Task OnPreExecuteAsync(IQuery query)
        {
            if(query is MockQuery testQuery)
            { 
                testQuery.QueryAsserts.Add("QueryFilterTwo-Pre");
            }

            return Task.CompletedTask;
        }

        public Task OnPostExecuteAsync(IQuery query)
        {
            if (query is MockQuery testQuery)
            { 
                testQuery.QueryAsserts.Add("QueryFilterTwo-Post");
            }

            return Task.CompletedTask;
        }
    }
}
