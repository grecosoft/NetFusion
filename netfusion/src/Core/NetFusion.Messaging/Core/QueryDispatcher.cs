using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// Responsible for dispatching a query to its consumer and invoking
    /// any registered query filters.
    /// </summary>
    public class QueryDispatcher 
    {
        private readonly ILogger<QueryDispatcher> _logger;
        private readonly IServiceProvider _services;
        
        // Dependent modules.
        private readonly IQueryDispatchModule _dispatchModule;
        private readonly IQueryFilterModule _filterModule;

        public QueryDispatcher(ILogger<QueryDispatcher> logger, IServiceProvider services, 
            IQueryDispatchModule dispatchModule,
            IQueryFilterModule filterModule)
        {
            _logger = logger;
            _services = services;
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
                await InvokeDispatcher(dispatchInfo, query, cancellationToken).ConfigureAwait(false);
            }
            catch (QueryDispatchException ex)
            {
                // Log the details of the dispatch exception and rethrow.
                _logger.LogErrorDetails(MessagingLogEvents.MessagingException, ex, "Exception dispatching query.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(MessagingLogEvents.MessagingException, ex, "Unexpected Exception dispatching query.");
                throw;
            }

            return query.Result;
        }

        // Creates an instance of the consumer that will execute the query and calls it between the
        // pre and post filters.
        private async Task InvokeDispatcher(QueryDispatchInfo dispatcher, IQuery query, CancellationToken cancellationToken)
        {
            var consumer = (IQueryConsumer)_services.GetRequiredService(dispatcher.ConsumerType);

            var preFilters = _filterModule.QueryFilterTypes.Select(qf => _services.GetRequiredService(qf)).OfType<IPreQueryFilter>();
            var postFilters = _filterModule.QueryFilterTypes.Select(qf => _services.GetRequiredService(qf)).OfType<IPostQueryFilter>();
        
            LogQueryDispatch(query, consumer);

            await ApplyFilters(query, preFilters, (f, q) => ((IPreQueryFilter)f).OnPreExecute(q));
            await dispatcher.Dispatch(query, consumer, cancellationToken);
            await ApplyFilters(query, postFilters, (f, q) => ((IPostQueryFilter)f).OnPostExecute(q));
        }

        private void LogQueryDispatch(IQuery query, IQueryConsumer consumer)
        {
            _logger.LogTraceDetails($"Dispatching Query Type: {query.GetType()} to Consumer: {consumer.GetType()}", query);
        }
        
        // Executes a list of asynchronous filters and awaits their completion.  Once completed,
        // any task error(s) are checked and raised.
        private async Task ApplyFilters(IQuery query, IEnumerable<IQueryFilter> filters, Func<IQueryFilter, IQuery, Task> executeFilter)
        {
            filters = filters.ToArray();
            
            _logger.LogTraceDetails(MessagingLogEvents.QueryDispatch, "Applying Query Filters",
                new {
                    FilterTypes = filters.Select(f => f.GetType().FullName).ToArray()
                });

            TaskListItem<IQueryFilter>[] taskList = null;

            try
            {
                taskList = filters.Invoke(query, executeFilter);
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
