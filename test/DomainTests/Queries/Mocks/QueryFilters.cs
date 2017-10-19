using System.Threading.Tasks;
using NetFusion.Domain.Patterns.Queries;
using NetFusion.Domain.Patterns.Queries.Filters;

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
