using Autofac;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// Responsible for dispatching a query to its consumer and invoking
    /// any registered query filters.
    /// </summary>
    public class QueryDispatcher 
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
            if (query == null) throw new ArgumentNullException(nameof(query),
                "Query to dispatch can't be null.");

            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken),
                "Cancellation token can't be null.");

            QueryDispatchInfo dispatchInfo = _dispatchModule.GetQueryDispatchInfo(query.GetType());

            try
            {
                await InvokeDispatcher(dispatchInfo, query, cancellationToken);
            }
            catch (Exception ex)
            {
                var details = new {
                    Query = query,
                    DispatchInfo = dispatchInfo
                };

                throw new QueryDispatchException("Error dispatching query.", details, ex);
            }

            return query.Result;
        }

        // Creates an instance of the consumer that will execute the query and calls it between the
        // pre and post filters.
        private async Task InvokeDispatcher(QueryDispatchInfo dispatcher, IQuery query, CancellationToken cancellationToken)
        {
            var consumer = (IQueryConsumer)_lifetimeScope.Resolve(dispatcher.ConsumerType);

            var preFilters = _filterModule.PreFilterTypes.Select(qf => _lifetimeScope.Resolve(qf)).OfType<IQueryFilter>();
            var postFilters = _filterModule.PostFilterTypes.Select(qf => _lifetimeScope.Resolve(qf)).OfType<IQueryFilter>();

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
                    var filterErrors = taskList.GetExceptions(ti => new QueryFilterException(ti));
                    if (filterErrors.Any())
                    {
                        throw new QueryDispatchException("Exception when invoking query filters.", filterErrors);
                    }
                }

                throw new QueryDispatchException("Exception when invoking query filters.", ex);
            }
        }
    }
}
