using Autofac;
using NetFusion.Common;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Domain.Patterns.Queries.Filters;
using NetFusion.Domain.Patterns.Queries.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Domain.Patterns.Queries.Dispatch
{
    /// <summary>
    /// Responsible for dispatching a query to its consumer and invoking
    /// any registered query filters.
    /// </summary>
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly ILifetimeScope _lifetimeScope;
        
        // Dependent modules.
        private readonly IQueryDispatchModule _dispatchModule;
        private readonly IQueryFilterModule _filterModule;

        public QueryDispatcher(ILifetimeScope lifetimeScope, 
            IQueryDispatchModule dispatchModule,
            IQueryFilterModule filterModule)
        {
            _lifetimeScope = lifetimeScope;
            _dispatchModule = dispatchModule;
            _filterModule = filterModule;
        }

        public async Task<TResult> Dispatch<TResult>(IQuery<TResult> query, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(query, nameof(query), "query to dispatch can't be null.");

            QueryDispatchInfo dispatchInfo = _dispatchModule.GetQueryDispatchInfo(query.GetType());

            try { await InvokeDispatcher(dispatchInfo, query, cancellationToken); }
            catch (Exception ex)
            {
                throw new QueryDispatchException("Error dispatching query.", query, ex);
            }

            return query.Result;
        }

        // Creates an instance of the consumer that will execute the query and calls it after invoking the
        // pre-filters and before the post-filters.
        private async Task InvokeDispatcher(QueryDispatchInfo dispatcher, IQuery query, CancellationToken cancellationToken)
        {
            var consumer = (IQueryConsumer)_lifetimeScope.Resolve(dispatcher.ConsumerType);

            var preFilters = _filterModule.PreFilterTypes.Select(ft => _lifetimeScope.Resolve(ft)).OfType<IQueryFilter>();
            var postFilters = _filterModule.PostFilterTypes.Select(ft => _lifetimeScope.Resolve(ft)).OfType<IQueryFilter>();

            await ApplyFilters(query, preFilters);
            await dispatcher.Dispatch(query, consumer, cancellationToken);
            await ApplyFilters(query, postFilters);
        }
        
        // Executes a list of asynchronous filters and awaits their completion.  Once completed,
        // any task error(s) are checked and raised.
        private async Task ApplyFilters(IQuery query, IEnumerable<IQueryFilter> filters)
        {
            TaskListItem<IQueryFilter>[] taskList = null;

            try
            {
                taskList = filters.Invoke(query, (filter, q) => filter.OnExecute(q));
                await taskList.WhenAll();
            }
            catch (Exception ex)
            {
                if (taskList != null)
                {
                    var filterErrors = taskList.GetExceptions(fr => new QueryFilterException(fr));
                    if (filterErrors.Any())
                    {
                        throw new QueryDispatchException("Exception when invoking query filters.", filterErrors);
                    }

                    throw new QueryDispatchException("Exception when invoking query filters.", ex);
                }

                throw new QueryDispatchException("Exception when invoking query filters.", ex);
            }
        }
    }
}
