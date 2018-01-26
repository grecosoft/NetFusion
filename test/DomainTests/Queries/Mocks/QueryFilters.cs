using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Types;
using System.Threading.Tasks;

namespace DomainTests.Queries.Mocks
{
    public class QueryFilterOne : IQueryFilter
    {
        public Task OnExecute(IQuery query)
        {
            if (query is TestQuery testQuery)
            {
                testQuery.TestLog.Add(nameof(QueryFilterOne));
            }

            return Task.CompletedTask;
        }
    }

    public class QueryFilterTwo : IQueryFilter
    {
        public Task OnExecute(IQuery query)
        {
            if(query is TestQuery testQuery)
            {
                testQuery.TestLog.Add(nameof(QueryFilterTwo));
            }

            return Task.CompletedTask;
        }
    }
}
